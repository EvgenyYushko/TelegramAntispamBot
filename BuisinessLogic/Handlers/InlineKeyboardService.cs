using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuisinessLogic.Services;
using ServiceLayer.Models;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Infrastructure.Common.BotSettings;
using static Infrastructure.Helpers.Logger;

namespace BuisinessLogic.Handlers
{
	public partial class HandleMessageService : IHandleMessageService
	{
		private const string BACK = "back";
		private const string HELP_CHAT = "help_chat";
		private const string HUM = "hum";
		private const string OPEN_CHATS = "open_chats";
		private const string OPEN_SETTINGS = "open_settngs";
		private const string RE_TRAIN_MODEL = "re_train_model";

		private const string SPAM = "spam";

		private static readonly InlineKeyboardButton[] myChatsButton = { InlineKeyboardButton.WithCallbackData("📂 Мои чаты", OPEN_CHATS) };
		private static readonly InlineKeyboardButton[] connectSettingButton = { InlineKeyboardButton.WithCallbackData("Подключить защиту", OPEN_SETTINGS) };
		private static readonly InlineKeyboardButton[] helpChatButton = { InlineKeyboardButton.WithCallbackData("Помощь боту", HELP_CHAT) };
		private static readonly InlineKeyboardButton[] reTrainModelButton = { InlineKeyboardButton.WithCallbackData("Переобучить модель", RE_TRAIN_MODEL) };

		private async Task CallBackHandler(Update update, CancellationToken cancellationToken)
		{
			var callbackQuery = update.CallbackQuery;
			var userId = callbackQuery.From.Id;
			if (callbackQuery.Data == OPEN_SETTINGS)
			{
				await using (new WaitDialog(_telegramClient, userId).Show())
				{
					var settingsBoard = new InlineKeyboardMarkup(new[]
					{
						myChatsButton
					});

					await _telegramClient.EditMessageTextAsync(userId, callbackQuery.Message.MessageId,
						ChatSettingsInfo,
						replyMarkup: settingsBoard,
						parseMode: ParseMode.Html,
						disableWebPagePreview: true);
				}
			}

			if (callbackQuery.Data == OPEN_CHATS)
			{
				await using (new WaitDialog(_telegramClient, userId).Show())
				{
					await _telegramClient.EditMessageTextAsync(userId, callbackQuery.Message.MessageId,
						ChatSettingsInfo,
						replyMarkup: GetChatsBoard(userId),
						parseMode: ParseMode.Html,
						disableWebPagePreview: true);
				}
			}
			else if (callbackQuery.Data == BACK)
			{
				await using (new WaitDialog(_telegramClient, userId).Show())
				{
					await SendChoseChats(_telegramClient, update, cancellationToken, true);
				}
			}
			else if (callbackQuery.Data == HELP_CHAT)
			{
				await using (new WaitDialog(_telegramClient, userId).Show())
				{
					await SendHelpChats(_telegramClient, update, cancellationToken);
				}
			}
			else if (callbackQuery.Data.StartsWith(SPAM) || callbackQuery.Data.StartsWith(HUM))
			{
				await using (new WaitDialog(_telegramClient, userId).Show())
				{
					await ChekedSpamMsg(callbackQuery.Data.StartsWith(SPAM), update, callbackQuery, cancellationToken);
				}
			}
			else if (callbackQuery.Data == RE_TRAIN_MODEL)
			{
				if (_updateMLProcess)
				{
					return;
				}

				try
				{
					_updateMLProcess = true;
					var msg = "Сообщений для обновления датасета нету.";
					await using (new WaitDialog(_telegramClient, userId).Show())
					{
						if (await _mLFacade.RetrainModel())
						{
							msg = "Модель успешно обучена. Дата сет и модель обновлены на гугл диске";
						}
					}

					await _telegramClient.EditMessageTextAsync(userId, callbackQuery.Message.MessageId, msg,
						replyMarkup: InlineKeyboardButton.WithCallbackData("🔙 Назад", BACK),
						parseMode: ParseMode.Html,
						disableWebPagePreview: true);
				}
				finally
				{
					_updateMLProcess = false;
				}
			}
		}

		private async Task ChekedSpamMsg(bool isSpam, Update update, CallbackQuery callbackQuery,
			CancellationToken cancellationToken)
		{
			Log("ChekedSpamMsg - Start");
			var parts = callbackQuery.Data.Split('_');
			var messageId = Guid.Parse(parts[1]); // Получаем msg.Id
			Log($"ChekedSpamMsg - Proc 1 {messageId}");

			await _mLService.UpdateSuspiciousMessages(new SuspiciousMessageDto
			{
				Id = messageId,
				IsSpamByUser = isSpam,
				NeedsManualReview = false
			});
			Log("ChekedSpamMsg - Proc 2");

			await SendHelpChats(_telegramClient, update, cancellationToken);
			Log("ChekedSpamMsg - End");
		}

		private async Task SendChoseChats(ITelegramBotClient botClient, Update update, CancellationToken token,
			bool edit)
		{
			var allTgUsers = _telegramUserService.GetAllTelegramUsers();
			var allChars = _telegramUserService.GetAllChats();
			var allTgBannedUsers = _telegramUserService.GetAllBanedUsers();

			var userId = update.Message?.From.Id ?? update.CallbackQuery.From.Id;

			if (edit)
			{
				await _telegramClient.EditMessageTextAsync(userId, update.CallbackQuery.Message.MessageId,
					StartInfo(allTgUsers.Count, allChars.Count, allTgBannedUsers.Count),
					replyMarkup: GetMainMenuBoard(userId),
					parseMode: ParseMode.Html,
					disableWebPagePreview: true);
			}
			else
			{
				await botClient.SendTextMessageAsync(userId,
					StartInfo(allTgUsers.Count, allChars.Count, allTgBannedUsers.Count)
					, parseMode: ParseMode.Html, disableWebPagePreview: true,
					cancellationToken: token, replyMarkup: GetMainMenuBoard(userId));
			}
		}

		private async Task SendHelpChats(ITelegramBotClient botClient, Update update, CancellationToken token)
		{
			var callbackQuery = update.CallbackQuery;
			var userId = callbackQuery.From.Id;
			var msgs = await _mLService.GetAllSuspiciousMessages();
			var msg = msgs.FirstOrDefault(m => m.IsSpamByUser is null && m.NeedsManualReview);
			if (msg is null)
			{
				await _telegramClient.EditMessageTextAsync(userId, callbackQuery.Message.MessageId,
					"Сообщений для анализа пока нету.",
					replyMarkup: InlineKeyboardButton.WithCallbackData("🔙 Назад", BACK),
					parseMode: ParseMode.Html,
					disableWebPagePreview: true);

				return;
			}

			var percent = Math.Round(msg.Probability * 100, 2).ToString().Replace(".", ",");
			var text = $"Создано: {msg.CreatedAt.ToString("MM.dd HH:mm:ss.fff")}\n" +
					   $"Модель: {(msg.IsSpamByMl ? "Спам" : "Не спам")} - {percent}%\n" +
					   $"Gemini:  {(msg.IsSpamByGemini.Value ? "Спам" : "Не спам")}" +
						"\n\n" + msg.Text;

			if (callbackQuery.Message.Text != text)
			{
				await _telegramClient.EditMessageTextAsync(userId, callbackQuery.Message.MessageId,
					text,
					replyMarkup: GetSpamHumBoards(msg.Id),
					parseMode: ParseMode.Html,
					disableWebPagePreview: true);
			}

			//await botClient.SendTextMessageAsync(userId, msg.Text, parseMode: ParseMode.Html, disableWebPagePreview: true,
			//	cancellationToken: token, replyMarkup: GetSpamHumBoards());
		}

		private InlineKeyboardMarkup GetMainMenuBoard(long userId)
		{
			var userChats = _telegramUserService.GetChatsByUser(userId);

			if (userId == ADMIN_ID)
			{
				return new InlineKeyboardMarkup(new[]
				{
					connectSettingButton,
					myChatsButton,
					helpChatButton,
					reTrainModelButton
				});
			}

			if (userChats is not null && userChats.Any(c => c.AdminsIds.Contains(userId) || c.CreatorId.Equals(userId)))
			{
				return new InlineKeyboardMarkup(new[]
				{
					connectSettingButton,
					myChatsButton,
					helpChatButton
				});
			}

			return new InlineKeyboardMarkup(new[]
			{
				connectSettingButton,
				myChatsButton
			});
		}

		private static InlineKeyboardButton[] CreateSpamHamButtons(Guid id)
		{
			return new[]
			{
				InlineKeyboardButton.WithCallbackData("Спам!", SPAM + $"_{id}"),
				InlineKeyboardButton.WithCallbackData("Не спам", HUM + $"_{id}")
			};
		}

		private static InlineKeyboardMarkup GetSpamHumBoards(Guid id)
		{
			// Создаём массив массивов кнопок
			var buttons = new[]
			{
				// Первый ряд: кнопки "Спам" и "Не спам"
				CreateSpamHamButtons(id),
				// Второй ряд: кнопка "Назад"
				new[] { InlineKeyboardButton.WithCallbackData("🔙 Назад", BACK) }
			};

			return new InlineKeyboardMarkup(buttons);
		}

		private InlineKeyboardMarkup GetChatsBoard(long userId)
		{
			var userChats = _telegramUserService.GetChatsByUser(userId);
			var buttons = new List<InlineKeyboardButton[]>();
			if (userChats is not null)
			{
				foreach (var chat in userChats)
				{
					if (chat.AdminsIds.Contains(userId) || chat.CreatorId.Equals(userId))
					{
						var chatLink = $"https://telegramantispambot.onrender.com/ChatProfile?chatId={chat.TelegramChatId}";
						buttons.Add(new[] { InlineKeyboardButton.WithUrl($"{chat.Title}", chatLink) });
					}
				}
			}

			buttons.Add(new[]
			{
				InlineKeyboardButton.WithUrl("Добавить бота в чат", inviteLink),
				InlineKeyboardButton.WithUrl("Добавить бота в канал", inviteLink)
			});

			buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("🔙 Назад", BACK) });

			var newKeyboard = new InlineKeyboardMarkup(buttons);
			return newKeyboard;
		}
	}
}