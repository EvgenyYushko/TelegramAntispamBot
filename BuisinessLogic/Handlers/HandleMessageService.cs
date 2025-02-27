﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Common;
using Infrastructure.Extentions;
using Infrastructure.InjectSettings;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Infrastructure.Helpers.TelegramUserHelper;
using static Infrastructure.Common.TimeZoneHelper;

namespace BuisinessLogic.Handlers
{
	public class HandleMessageService : IHandleMessageService
	{
		private readonly IDeleteMessageService _deleteMessageService;
		private readonly IProfanityCheckerService _profanityCheckerService;
		private readonly ITelegramUserService _telegramUserService;
		TelegramBotClient _telegramClient;

		public HandleMessageService(IDeleteMessageService deleteMessageService, IProfanityCheckerService profanityCheckerService, ITelegramUserService telegramUserService)
		{
			_deleteMessageService = deleteMessageService;
			_profanityCheckerService = profanityCheckerService;
			_telegramUserService = telegramUserService;
		}

		/// <inheritdoc />
		public async Task HandleUpdateAsync(TelegramInject botClient, Update update, UpdateType type, CancellationToken cancellationToken)
		{
			if (update.Message is not null)
			{
				await _telegramUserService.TryAdd(new()
				{
					UserId = update.Message.From.Id,
					Name = update.Message.From.Username,
					CreateDate = DateTimeNow,
					User = update.Message.From
				});
			}

			_telegramClient = botClient.TelegramClient;

			if (update.HasEmptyMessage())
			{
				await HandleChatMemberUpdateAsync(_telegramClient, update, cancellationToken);
				return;
			}

			switch (type)
			{
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
				default:
					return; 
			}
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

			var userInfo = _telegramUserService.Get(user.Id);

			userInfo.PullModel.PullTimer = new Timer(OnTimerElapsed, user.Id, TimeSpan.FromSeconds(20), Timeout.InfiniteTimeSpan);
			userInfo.PullModel.Message = update.Message;
			userInfo.PullModel.PollMessageId = pollMessage.MessageId;
			userInfo.PullModel.PullId = pollMessage.Poll.Id;
		}

		private void OnTimerElapsed(object state)
		{
			var userId = (long)state;
			var user = _telegramUserService.Get(userId);

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
