using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuisinessLogic.Services;
using Infrastructure.Common;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Infrastructure.Common.BotSettings;

namespace BuisinessLogic.Handlers
{
	public partial class HandleMessageService : IHandleMessageService
	{
		private const string OPEN_CHATS = "open_chats";
		private const string OPEN_SETTINGS = "open_settngs";
		private const string BACK = "back";
		private const string HELP_CHAT = "help_chat";
		private const string RE_TRAIN_MODEL = "re_train_model";

		private const string SPAM = "spam";
		private const string HUM = "hum";

		private static InlineKeyboardButton[] myChatsButton = new[] { InlineKeyboardButton.WithCallbackData("📂 Мои чаты", OPEN_CHATS) };
		private static InlineKeyboardButton[] connectSettingButton = new[] { InlineKeyboardButton.WithCallbackData("Подключить защиту", OPEN_SETTINGS) };
		private static InlineKeyboardButton[] helpChatButton = new[] { InlineKeyboardButton.WithCallbackData("Помощь боту", HELP_CHAT) };
		private static InlineKeyboardButton[] reTrainModelButton = new[] { InlineKeyboardButton.WithCallbackData("Переобучить модель", RE_TRAIN_MODEL) };

		private async Task CallBackHandler(Update update, CancellationToken cancellationToken)
		{
			var callbackQuery = update.CallbackQuery;
			var userId = callbackQuery.From.Id;
			if (callbackQuery.Data == OPEN_SETTINGS)
			{
				using (new WaitDialg(_telegramClient, userId).Show())
				{
					var settingsBoard = new InlineKeyboardMarkup(new[]
					{
						myChatsButton
					});

					await _telegramClient.EditMessageTextAsync(userId, callbackQuery.Message.MessageId,
						 BotSettings.ChatSettingsInfo,
						 replyMarkup: settingsBoard,
						 parseMode: ParseMode.Html,
						 disableWebPagePreview: true);
				}
			}
			if (callbackQuery.Data == OPEN_CHATS)
			{
				await _telegramClient.EditMessageTextAsync(userId, callbackQuery.Message.MessageId,
					 BotSettings.ChatSettingsInfo,
					 replyMarkup: GetChatsBoard(userId),
					 parseMode: ParseMode.Html,
					 disableWebPagePreview: true);
			}
			else if (callbackQuery.Data == BACK)
			{
				using (new WaitDialg(_telegramClient, userId).Show())
				{
					await SendChoseChats(_telegramClient, update, cancellationToken, true);
				}
			}
			else if (callbackQuery.Data == HELP_CHAT)
			{
				using (new WaitDialg(_telegramClient, userId).Show())
				{
					await SendHelpChats(_telegramClient, update, cancellationToken);
				}
			}
			else if (callbackQuery.Data.StartsWith(SPAM) || callbackQuery.Data.StartsWith(HUM))
			{
				using (new WaitDialg(_telegramClient, userId).Show())
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
					using (new WaitDialg(_telegramClient, userId).Show())
					{
						var isUpdated = await _mLService.UpdateDataSet();
						if (isUpdated)
						{
							//await Task.Delay(120000);
							await _spamDetector.TrainModelAsync();
							await _mLService.UploadModelAndDataSetToDrive();
							await _mLService.DeleteAllSuspiciousMessages();
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

		private async Task ChekedSpamMsg(bool isSpam, Update update, CallbackQuery callbackQuery, CancellationToken cancellationToken)
		{
			var parts = callbackQuery.Data.Split('_');
			var messageId = Guid.Parse(parts[1]); // Получаем msg.Id

			await _mLService.UpdateSuspiciousMessages(new ServiceLayer.Models.SuspiciousMessageDto()
			{
				Id = messageId,
				IsSpamByUser = isSpam,
				NeedsManualReview = false
			});
			await SendHelpChats(_telegramClient, update, cancellationToken);
		}

		private async Task SendChoseChats(ITelegramBotClient botClient, Update update, CancellationToken token, bool edit)
		{
			var allTgUsers = _telegramUserService.GetAllTelegramUsers();
			var allChars = _telegramUserService.GetAllChats();
			var allTgBannedUsers = _telegramUserService.GetAllBanedUsers();

			var userId = update.Message?.From.Id ?? update.CallbackQuery.From.Id;

			if (edit)
			{
				await _telegramClient.EditMessageTextAsync(userId, update.CallbackQuery.Message.MessageId,
							BotSettings.StartInfo(allTgUsers.Count, allChars.Count, allTgBannedUsers.Count),
							replyMarkup: GetMainMenuBoard(userId),
							parseMode: ParseMode.Html,
							disableWebPagePreview: true);
			}
			else
			{
				await botClient.SendTextMessageAsync(userId, BotSettings.StartInfo(allTgUsers.Count, allChars.Count, allTgBannedUsers.Count)
					, parseMode: ParseMode.Html, disableWebPagePreview: true,
					cancellationToken: token, replyMarkup: GetMainMenuBoard(userId));
			}
		}

		private async Task SendHelpChats(ITelegramBotClient botClient, Update update, CancellationToken token)
		{
			var callbackQuery = update.CallbackQuery;
			var userId = callbackQuery.From.Id;
			var msgs = _mLService.GetAllSuspiciousMessages();
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

			var percent = (msg.Probability * 100).ToString("0.00%").Replace(".", ",");
			var text = $"Модель: {(msg.IsSpamByMl ? "Спам" : "Не спам")}, вероятность = {percent}:\n" +
					   $"Gemini: {(msg.IsSpamByGemini.Value ? "Спам" : "Не спам")}" +
					   $"\n\n" + msg.Text;

			Console.WriteLine(text);

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
						buttons.Add(new[] { InlineKeyboardButton.WithCallbackData($"{chat.Title}", $"chat_{chat.TelegramChatId}") });
					}
				}
			}

			buttons.Add(new InlineKeyboardButton[]
			{
				InlineKeyboardButton.WithUrl("Добавить бота в чат", BotSettings.inviteLink),
				InlineKeyboardButton.WithUrl("Добавить бота в канал", BotSettings.inviteLink)
			});

			buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("🔙 Назад", BACK) });

			var newKeyboard = new InlineKeyboardMarkup(buttons);
			return newKeyboard;
		}

	}
}
