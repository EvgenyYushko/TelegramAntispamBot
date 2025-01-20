using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAntispamBot.InjectSettings;
using TelegramAntispamBot.ServiceLayer.Services;

namespace TelegramAntispamBot.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class BotController : ControllerBase
	{
		private readonly IHandleMessageService _messageService;
		private readonly TelegramInject _botClient;

		public BotController(IHandleMessageService messageService, TelegramInject telegram)
		{
			_messageService = messageService;
			_botClient = telegram;
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] Update update)
		{
			Console.WriteLine("Invike method Post()");

			try
			{
				var token = new CancellationToken();
				await _messageService.HandleUpdateAsync(_botClient, update, update.Type, token);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
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
				cancellationToken: cts.Token
			);
			_botClient.TelegramClient.OnApiResponseReceived += BotClient_OnApiResponseReceived;
			_botClient.TelegramClient.OnMakingApiRequest += BotClient_OnMakingApiRequest;
		}

		private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			await Post(update);
		}

		private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
		{
			var errorMessage = exception switch
			{
				ApiRequestException apiRequestException
					=> $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
				_ => exception.ToString()
			};
			Console.WriteLine(errorMessage);
			return Task.CompletedTask;
		}

		private static ValueTask BotClient_OnMakingApiRequest(ITelegramBotClient botClient,
			Telegram.Bot.Args.ApiRequestEventArgs args, CancellationToken cancellationToken = default) => default;

		private static ValueTask BotClient_OnApiResponseReceived(ITelegramBotClient botClient,
			Telegram.Bot.Args.ApiResponseEventArgs args, CancellationToken cancellationToken = default) => default;

		#endregion
	}
}
