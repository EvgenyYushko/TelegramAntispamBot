﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAntispamBot.Common;
using TelegramAntispamBot.Extentions;
using TelegramAntispamBot.ServiceLayer.Services;

namespace TelegramAntispamBot.BuisinessLogic.Services
{
	public class HandleMessageService : IHandleMessageService
	{
		private readonly IDeleteMessageService _deleteMessageService;
		private readonly IProfanityCheckerService _profanityCheckerService;

		public HandleMessageService(IDeleteMessageService deleteMessageService, IProfanityCheckerService profanityCheckerService)
		{
			_deleteMessageService = deleteMessageService;
			_profanityCheckerService = profanityCheckerService;
		}

		/// <inheritdoc />
		public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, UpdateType type, CancellationToken cancellationToken)
		{
			if (update.HasEmptyMessage())
			{
				await HandleChatMemberUpdateAsync(botClient, update, cancellationToken);
				return;
			}

			switch (type)
			{
				case UpdateType.Message when _profanityCheckerService.ContainsProfanity(update.Message.Text):
					await _deleteMessageService.DeleteMessageAsync(botClient, update.Message, cancellationToken, BotSettings.InfoMessageProfanityChecker);
					break;
				// Disable comments if new post contains no-comment word
				case UpdateType.Message when update.Message.From.IsChannel() &&
											 update.Message.Text.Contains(BotSettings.NoCommentWord):
				// Delete new comment with link if user not in white-list
				case UpdateType.Message when update.Message.ContainsUrls() &&
											 !update.Message.From.IsBot &&
											 !update.Message.From.IsChannel() &&
											 !update.Message.From.InWhitelist():
				// Delete new community-comment if user not in white-list
				case UpdateType.Message when update.Message.From.IsBot &&
											 !update.Message.SenderChat.InChannelsWhitelist():
					await _deleteMessageService.DeleteMessageAsync(botClient, update.Message, cancellationToken, BotSettings.InfoMessage);
					break;
				case UpdateType.Message when update.Message.Text.Equals("/help") || update.Message.Text.Equals("/help@YN_AntispamBot"):
					await SendWelcomeMessage(botClient, update, cancellationToken, update.Message.From);
					break;
				// Disable comments if edited post contains no-comment word
				case UpdateType.EditedMessage when update.EditedMessage.From.IsChannel() &&
												   update.EditedMessage.Text.Contains(BotSettings.NoCommentWord):
				// Delete edited comment with link if user not in white-list
				case UpdateType.EditedMessage when update.EditedMessage.ContainsUrls() &&
												   !update.EditedMessage.From.IsBot &&
												   !update.EditedMessage.From.IsChannel() &&
												   !update.EditedMessage.From.InWhitelist():
				// Delete edited community-comment if user not in white-list
				case UpdateType.EditedMessage when update.EditedMessage.From.IsBot &&
												   !update.EditedMessage.SenderChat.InChannelsWhitelist():
					await _deleteMessageService.DeleteMessageAsync(botClient, update.EditedMessage, cancellationToken, BotSettings.InfoMessage);
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
			await botClient.SendTextMessageAsync(update.Message.Chat.Id, BotSettings.GetWelcomeMessage(user), cancellationToken: token);
		}
	}
}
