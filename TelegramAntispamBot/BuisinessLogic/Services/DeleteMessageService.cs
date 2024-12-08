using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramAntispamBot.DataAccessLayer;
using TelegramAntispamBot.ServiceLayer.Services;

namespace TelegramAntispamBot.BuisinessLogic.Services
{
	public class DeleteMessageService : IDeleteMessageService
	{
		public async Task DeleteMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
		{
			var chatId = message.Chat.Id;
			var messageText = message.Text;
			var messageId = message.MessageId;
			Console.WriteLine($"Message for delete: '{messageText}' (id: {messageId})");

			await botClient.DeleteMessageAsync(chatId, messageId, cancellationToken);

			if (message.From is { Username: { } })
			{
				await botClient.SendTextMessageAsync(message.From.Id, BotSettings.InfoMessage);
			}
		}
	}
}
