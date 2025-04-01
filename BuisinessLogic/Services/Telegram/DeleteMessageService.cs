using System;
using System.Threading;
using System.Threading.Tasks;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static Infrastructure.Helpers.Logger;

namespace BuisinessLogic.Services.Telegram
{
	public class DeleteMessageService : IDeleteMessageService
	{
		public async Task DeleteMessageAsync(ITelegramBotClient botClient, Message message,
			CancellationToken cancellationToken, string msg, InlineKeyboardMarkup inlineKeyboard)
		{
			var chatId = message.Chat.Id;
			var messageText = message.Text;
			var messageId = message.MessageId;

			Log($"Message for delete: '{messageText}' (id: {messageId})");
			await botClient.DeleteMessageAsync(chatId, messageId, cancellationToken);

			if (message.From is { Username: { } })
			{
				try
				{
					await botClient.SendTextMessageAsync(message.From.Id, msg, cancellationToken: cancellationToken,
						replyMarkup: inlineKeyboard);
				}
				catch (ApiRequestException ex) when (ex.ErrorCode == 403)
				{
					Log("User does not contact the bot or has blocked it");
					await botClient.SendTextMessageAsync(chatId, msg, cancellationToken: cancellationToken,
						replyMarkup: inlineKeyboard);
				}
				catch (Exception e)
				{
					Log($"User error send message after delete message: {e}");
				}
			}
		}
	}
}