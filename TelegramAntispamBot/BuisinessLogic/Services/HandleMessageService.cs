using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAntispamBot.DataAccessLayer;
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
				return;
			}

			var newMember = update.ChatMember?.NewChatMember;
			var oldMember = update.ChatMember?.OldChatMember;

			// Проверяем, если пользователь добавлен
			if (newMember is ChatMemberMember && oldMember is ChatMemberLeft)
			{
				string welcomeMessage = $"Добро пожаловать, {newMember.User.FirstName}!";
				await botClient.SendTextMessageAsync(
					chatId: update.Message.Chat.Id,
					text: welcomeMessage,
					cancellationToken: cancellationToken
				);
			}

			switch (type)
			{
				case UpdateType.ChatMember when update.ChatMember is not null:
					await HandleChatMemberUpdateAsync(botClient, update.ChatMember , cancellationToken);
					break;
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

		private static async Task HandleChatMemberUpdateAsync(ITelegramBotClient botClient, ChatMemberUpdated chatMemberUpdate, CancellationToken cancellationToken)
		{
			var newMember = chatMemberUpdate.NewChatMember;
			var oldMember = chatMemberUpdate.OldChatMember;

			// Проверяем, если пользователь добавлен
			if (newMember is ChatMemberMember && oldMember is ChatMemberLeft)
			{
				var welcomeMessage = $"Добро пожаловать, {newMember.User.FirstName}!";
				await botClient.SendTextMessageAsync(chatId: chatMemberUpdate.Chat.Id, text: welcomeMessage, cancellationToken: cancellationToken);
			}
		}
	}
}
