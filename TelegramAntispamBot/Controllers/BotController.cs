using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.InjectSettings;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Infrastructure.Helpers.Logger;

namespace TelegramAntispamBot.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class BotController : ControllerBase
	{
		private readonly TelegramInject _botClient;
		private readonly IHandleMessageService _messageService;

		public BotController(IHandleMessageService messageService, TelegramInject telegram)
		{
			_messageService = messageService;
			_botClient = telegram;
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] Update update)
		{
			try
			{
				Log("Invike method Post()");
				Log($"{update.Message?.Text}");

				var token = new CancellationToken();
				await _messageService.HandleUpdateAsync(_botClient, update, update.Type, token);
			}
			catch (Exception e)
			{
				Log(e.ToString());
			}

			return Ok();
		}

		#region ForLocalTest

		public void RunLocalTest()
		{
			using var cts = new CancellationTokenSource();
			// Запуск бота с обработкой сообщений
			_botClient.TelegramClient.StartReceiving(
				HandleUpdateAsync,
				HandleErrorAsync,
				new ReceiverOptions
				{
					AllowedUpdates = Array.Empty<UpdateType>() // Обработка всех типов обновлений
				},
				cts.Token
			);
			_botClient.TelegramClient.OnApiResponseReceived += BotClient_OnApiResponseReceived;
			_botClient.TelegramClient.OnMakingApiRequest += BotClient_OnMakingApiRequest;
		}

		private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
			CancellationToken cancellationToken)
		{
			await Post(update);
		}

		private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
			CancellationToken cancellationToken)
		{
			var errorMessage = exception switch
			{
				ApiRequestException apiRequestException
					=> $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
				_ => exception.ToString()
			};
			Log(errorMessage);
			return Task.CompletedTask;
		}

		private static ValueTask BotClient_OnMakingApiRequest(ITelegramBotClient botClient,
			ApiRequestEventArgs args, CancellationToken cancellationToken = default)
		{
			return default;
		}

		private static ValueTask BotClient_OnApiResponseReceived(ITelegramBotClient botClient,
			ApiResponseEventArgs args, CancellationToken cancellationToken = default)
		{
			return default;
		}

		#endregion
	}
}