using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuisinessLogic.Services;
using Infrastructure.Common;
using Infrastructure.Enumerations;
using Infrastructure.Extentions;
using Infrastructure.InjectSettings;
using Infrastructure.Models;
using ML_SpamClassifier.Interfaces;
using ServiceLayer.Services.Authorization;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Infrastructure.Common.TimeZoneHelper;
using static Infrastructure.Helpers.TelegramUserHelper;

namespace BuisinessLogic.Handlers
{
	public class HandleMessageService : IHandleMessageService
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

		private readonly IDeleteMessageService _deleteMessageService;
		private readonly IProfanityCheckerService _profanityCheckerService;
		private readonly ITelegramUserService _telegramUserService;
		private readonly IUserService _userService;
		private readonly ISpamDetector _spamDetector;
		private readonly IMLService _mLService;
		TelegramBotClient _telegramClient;

		public HandleMessageService(IDeleteMessageService deleteMessageService, IProfanityCheckerService profanityCheckerService
			, ITelegramUserService telegramUserService
			, IUserService userService
			, ISpamDetector spamDetector
			, IMLService mLService
			)
		{
			_deleteMessageService = deleteMessageService;
			_profanityCheckerService = profanityCheckerService;
			_telegramUserService = telegramUserService;
			_userService = userService;
			_spamDetector = spamDetector;
			_mLService = mLService;
		}

		private static bool _firstRunBot = true;
		private static bool _updateMLProcess;

		/// <inheritdoc />
		public async Task HandleUpdateAsync(TelegramInject botClient, Update update, UpdateType type, CancellationToken cancellationToken)
		{
			_telegramClient = botClient.TelegramClient;

			if (_firstRunBot)
			{
				await _mLService.DownloadModel();
				await _spamDetector.LoadModel();
				await _telegramUserService.UpdateLocalStorage();
				_firstRunBot = false;
			}

			if (update.Message?.Text is not null && !update.Message.Text.StartsWith("/"))
			{
				string comment = null;
				var isSpam = _spamDetector.IsSpam(update.Message.Text, ref comment);
				if (isSpam && comment is not null)
				{
					//await _telegramClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
					await _telegramClient.SendTextMessageAsync(
					chatId: update.Message.Chat.Id,
					text: comment,
					replyToMessageId: update.Message.MessageId, // Цитируем сообщение пользователя
					cancellationToken: cancellationToken);

					return;
				}
			}

			//var me = await _telegramClient.GetMeAsync();
			//var test = await _telegramClient.GetChatMenuButtonAsync(update.Message.Chat.Id);

			if (update.Message != null && !update.Message.Type.Equals(MessageType.ChatMembersAdded) && update.Message.Chat.Type.Equals(ChatType.Supergroup) && type is not UpdateType.InlineQuery and not UpdateType.MyChatMember)
			{
				//var s = await _telegramClient.GetChatAsync(new ChatId(update.Message.Chat.Id), cancellationToken);
				var chatMember = await _telegramClient.GetChatMemberAsync(new ChatId(update.Message.Chat.Id), update.Message.From.Id, cancellationToken);
				var admins = await _telegramClient.GetChatAdministratorsAsync(new ChatId(update.Message.Chat.Id), cancellationToken);
				var creator = admins?.Where(a => a is ChatMemberOwner)
					.Select(c => new TelegramUser()
					{
						UserId = c.User.Id,
						Name = c.User.Username
					}).First();
				var adminsMember = admins?.Where(a => a is ChatMemberAdministrator adm && !adm.User.IsBot)
					.Select(a => new TelegramUser()
					{
						UserId = a.User.Id,
						Name = a.User.Username ?? (string.IsNullOrWhiteSpace(a.User.FirstName) ? "" : a.User.FirstName + " " + (string.IsNullOrWhiteSpace(a.User.LastName) ? "" : a.User.LastName))
					}).ToList();

				if (update.Message is not null)
				{
					await _telegramUserService.TryAdd(new()
					{
						UserId = update.Message.From.Id,
						Name = update.Message.From.Username,
						CreateDate = DateTimeNow,
						User = update.Message.From,
						Chanel = new Chanel()
						{
							TelegramChatId = update.Message.Chat.Id,
							ChatType = update.Message.Chat.Type.ToString(),
							Title = update.Message.Chat.Title,
							Creator = creator,
							CreatorId = creator.UserId,
							AdminsMembers = adminsMember,
							ChatMember = chatMember
						}
					});
				}

				foreach (var admin in admins.Where(a => a is ChatMember adm && !adm.User.IsBot))
				{
					var adminEntity = _telegramUserService.Get(admin.User.Id);
					if (adminEntity != null && adminEntity.UserSiteId != default)
					{
						await _userService.UpdateRole(adminEntity.UserSiteId, Role.Tutor);
					}
				}
			}

			//var chats = _telegramUserService.GetChatsByUser(update.Message.From.Id);

			if (update.Message is not null && update.Message.Type is MessageType.ChatMembersAdded or MessageType.ChatMemberLeft)
			{
				await _telegramClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
			}

			if (update.HasEmptyMessage())
			{
				//await HandleChatMemberUpdateAsync(_telegramClient, update, cancellationToken);
				return;
			}

			switch (type)
			{
				case UpdateType.Message when update.Message.Text.Equals("/start"):
					{
						using (new WaitDialg(_telegramClient, update.Message.From.Id).Show())
						{
							await SendChoseChats(_telegramClient, update, cancellationToken, false);
						}
					}
					break;
				case UpdateType.Message when BotSettings.IsFlooding(update.Message.From.Id):
					await _telegramClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId, cancellationToken);
					break;
				case UpdateType.Message when _profanityCheckerService.ContainsProfanity(update.Message.Text):
					await _deleteMessageService.DeleteMessageAsync(_telegramClient, update.Message, cancellationToken, BotSettings.InfoMessageProfanityChecker);
					if (!await _telegramUserService.CheckReputation(update.Message))
					{
						await SendPull(botClient, update, update.Message.From);
					}
					break;
				// Disable comments if new post contains no-comment word
				case UpdateType.Message when update.Message.From.IsChannel() &&
											 update.Message.Text.Contains(BotSettings.NoCommentWord):
				// Delete new comment with link if user not in white-list
				case UpdateType.Message when update.Message.ContainsUrls() &&
											 !update.Message.From.IsBot &&
											 !update.Message.From.IsChannel() &&
											 !await _telegramUserService.InWhitelist(update.Message.From.Id):
				// Delete new community-comment if user not in white-list
				case UpdateType.Message when update.Message.From.IsBot &&
											 !update.Message.SenderChat.InChannelsWhitelist():
					await _deleteMessageService.DeleteMessageAsync(_telegramClient, update.Message, cancellationToken, BotSettings.InfoMessage, BotSettings.LinkButton);
					break;
				case UpdateType.Message when update.Message.Text.Equals("/help") || update.Message.Text.Equals("/help@YN_AntispamBot"):
					await SendWelcomeMessage(_telegramClient, update, cancellationToken, update.Message.From);
					break;
				case UpdateType.Message when update.Message.Text.Equals("/allbannedusers") || update.Message.Text.Equals("/allbannedusers@YN_AntispamBot"):
					await GetAllBannedUsers(_telegramClient, update, cancellationToken, update.Message.From);
					break;
				case UpdateType.Message when update.Message.Text.Equals("/banrequest") || update.Message.Text.Equals("/banrequest@YN_AntispamBot"):
					await SendPull(botClient, update, update.Message.From);
					break;
				//case UpdateType.PollAnswer:
				//	SavePull(botClient, update);
				//	break;
				case UpdateType.Poll:
					await AnalizePull(update);
					break;
				// Disable comments if edited post contains no-comment word
				case UpdateType.EditedMessage when update.EditedMessage.From.IsChannel() &&
												   update.EditedMessage.Text.Contains(BotSettings.NoCommentWord):
				// Delete edited comment with link if user not in white-list
				case UpdateType.EditedMessage when update.EditedMessage.ContainsUrls() &&
												   !update.EditedMessage.From.IsBot &&
												   !update.EditedMessage.From.IsChannel() &&
												   !await _telegramUserService.InWhitelist(update.Message.From.Id):
				// Delete edited community-comment if user not in white-list
				case UpdateType.EditedMessage when update.EditedMessage.From.IsBot &&
												   !update.EditedMessage.SenderChat.InChannelsWhitelist():
					await _deleteMessageService.DeleteMessageAsync(_telegramClient, update.EditedMessage, cancellationToken, BotSettings.InfoMessage, BotSettings.LinkButton);
					break;
				case UpdateType.CallbackQuery:
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
							Console.WriteLine($"Start._updateMLProcess = {_updateMLProcess}");
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
										// todo удалить все данные для обучения
										msg = "Модель успешно обучена. Дата сет и модель обновлены на гугл диске";
									}
								}

								await _telegramClient.EditMessageTextAsync(userId, callbackQuery.Message.MessageId,
											msg,
											replyMarkup: InlineKeyboardButton.WithCallbackData("🔙 Назад", BACK),
											parseMode: ParseMode.Html,
											disableWebPagePreview: true);
							}
							finally
							{
								_updateMLProcess = false;
								Console.WriteLine($"finally._updateMLProcess = {_updateMLProcess}");
							}
						}

						break;
					}
				case UpdateType.MyChatMember:
					{
						if (update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Administrator)
						{
							if (update.MyChatMember.NewChatMember.User.IsBot && update.MyChatMember.NewChatMember.User.Username == "YN_AntispamBot")
							{
								await _telegramClient.SendTextMessageAsync(update.MyChatMember.Chat.Id, BotSettings.GetStartBOtWelcomeMessage(),
										cancellationToken: cancellationToken);
							}
						}
						break;
					}
				default:
					return;
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

		private static async Task HandleChatMemberUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			var newMember = update.Message.NewChatMembers?.FirstOrDefault();
			// Проверяем, если пользователь добавлен 
			if (newMember != null)
			{
				await SendWelcomeMessage(botClient, update, cancellationToken, newMember);
			}
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
			var text = $"Вероятность спама = {msg.Probability}:\n\n" + msg.Text;
			//var userId = update.Message?.From.Id ?? update.CallbackQuery.From.Id;
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

			if (userId == 1231047171)
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

		private static async Task SendWelcomeMessage(ITelegramBotClient botClient, Update update, CancellationToken token, User user)
		{
			await botClient.SendTextMessageAsync(update.Message.Chat.Id, BotSettings.GetWelcomeMessage(user),
				cancellationToken: token);
		}

		private async Task GetAllBannedUsers(ITelegramBotClient botClient, Update update, CancellationToken token, User user)
		{
			var banedUers = _telegramUserService.GetAllBanedUsers();
			var usersMsg = "Список заблокированных пользователей ботом за всё время :\n\n" + string.Join("\n", banedUers);

			await botClient.SendTextMessageAsync(update.Message.Chat.Id, usersMsg, cancellationToken: token);
		}

		private async Task SendPull(TelegramInject botClient, Update update, User user)
		{
			// Создание опроса
			var pollMessage = await botClient.TelegramClient.SendPollAsync(
				chatId: update.Message.Chat.Id,
				question: $"Удалить пользователя {user.Username} из чата за нарушения правил?",
				options: new[] { "Да ✅", "Нет ❌", "Себя удали!" },
				isAnonymous: false,
				allowsMultipleAnswers: false  // Только один вариант ответа
			);

			var userInfo = _telegramUserService.GetFromLocal(user.Id);

			userInfo.PullModel.PullTimer = new Timer(OnTimerElapsed, user.Id, TimeSpan.FromSeconds(20), Timeout.InfiniteTimeSpan);
			userInfo.PullModel.Message = update.Message;
			userInfo.PullModel.PollMessageId = pollMessage.MessageId;
			userInfo.PullModel.PullId = pollMessage.Poll.Id;
		}

		private void OnTimerElapsed(object state)
		{
			var userId = (long)state;
			var user = _telegramUserService.GetFromLocal(userId);

			var chatId = user.PullModel.Message.Chat.Id;
			var messageId = user.PullModel.PollMessageId;
			user.PullModel.PullTimer = null;
			user.PullModel.PullTimer?.Dispose();

			Task.Run(async () => await _telegramClient.StopPollAsync(chatId, messageId));
		}

		private async Task AnalizePull(Update update)
		{
			var pull = update.Poll;
			var ok = pull.Options[0];
			var no = pull.Options[1];

			var user = _telegramUserService.FindByPullId(pull.Id);
			if (user is null)
			{
				throw new Exception("User not finde by pullId");
			}

			if (user.PullModel.PullTimer is null)
			{
				if (ok.VoterCount > no.VoterCount)
				{
					if (ok.VoterCount > 1)
					{
						await _telegramClient.SendTextMessageAsync(user.PullModel.Message.Chat.Id, $"Большенство пользователей проголосовало за исключения " +
							$"{user.User.Username} из чата. \n\nПоэтому {user.User.Username} покидает чат!");

						try
						{
							await _telegramClient.BanChatMemberAsync(user.PullModel.Message.Chat.Id, user.PullModel.Message.From.Id);
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
						}

						try
						{
							await _telegramUserService.AddUserToBan(user);
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
						}
					}
					else
					{
						await _telegramClient.SendTextMessageAsync(user.PullModel.Message.Chat.Id, $"Голосование не сотоялось т.к. нужно больше одного голоса.");
					}
				}
				else
				{
					await _telegramClient.SendTextMessageAsync(user.PullModel.Message.Chat.Id, $"По результатам голосования пользователь " +
						$"{user.User.Username} остаётся в чате");
				}

				user.ResetPull();
			}
		}
	}


}
