using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
		private readonly TelegramBotClient _botClient;

		public BotController(IHandleMessageService messageService, TelegramInject telegram)
		{
			_messageService = messageService;
			_botClient = telegram.TelegramClient;
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] Update update)
		{
			Console.WriteLine($"Bot {await _botClient.GetMeAsync()} is running..."); ;

			try
			{
				var token = new CancellationToken();
				await _messageService.HandleUpdateAsync(_botClient, update, update.Type, token);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			return Ok();
		}

		[HttpGet("/health")]
		public IActionResult health()
		{
			return Ok();
		}

		#region ForLocalTest

		public void RunLocalTest()
		{
			using var cts = new CancellationTokenSource();
			// Запуск бота с обработкой сообщений
			_botClient.StartReceiving(
				HandleUpdateAsync,
				HandleErrorAsync,
				new ReceiverOptions
				{
					AllowedUpdates = Array.Empty<UpdateType>() // Обработка всех типов обновлений
				},
				cancellationToken: cts.Token
			);
			_botClient.OnApiResponseReceived += BotClient_OnApiResponseReceived;
			_botClient.OnMakingApiRequest += BotClient_OnMakingApiRequest;
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
