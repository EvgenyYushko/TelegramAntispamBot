﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using TelegramAntispamBot.ServiceLayer.Services;

namespace TelegramAntispamBot.BuisinessLogic.Services
{
	public class DeleteMessageService : IDeleteMessageService
	{
		public async Task DeleteMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string msg)
		{
			var chatId = message.Chat.Id;
			var messageText = message.Text; 
			var messageId = message.MessageId;
			Console.WriteLine($"Message for delete: '{messageText}' (id: {messageId})");
		
			await botClient.DeleteMessageAsync(chatId, messageId, cancellationToken);

			if (message.From is { Username: { } })
			{
				try
				{
					await botClient.SendTextMessageAsync(message.From.Id, msg, cancellationToken: cancellationToken);
				}
				catch (ApiRequestException ex) when (ex.ErrorCode == 403)
				{
					Console.WriteLine("Пользователь не взаимодействовал с ботом или заблокировал его.");
					await botClient.SendTextMessageAsync(chatId, msg, cancellationToken: cancellationToken);
				}
			}
		}
	}
}