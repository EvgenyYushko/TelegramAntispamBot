using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAntispamBot.ServiceLayer.Services;

namespace TelegramAntispamBot.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class BotController : ControllerBase
	{
		private readonly IHandleMessageService _messageService;
		private TelegramBotClient _botClient;

		public BotController(IConfiguration configuration, IHandleMessageService messageService)
		{
			_messageService = messageService;
			var botToken = configuration.GetValue<string>("TELEGRAM_ANTISPAM_BOT_KEY") ?? Environment.GetEnvironmentVariable("TELEGRAM_ANTISPAM_BOT_KEY");
			_botClient = new TelegramBotClient(botToken ?? throw new Exception("TelegrammToken is not be null"));

			Console.WriteLine(Environment.GetEnvironmentVariable("TELEGRAM_ANTISPAM_BOT_KEY")?? "NULL 1");
			Console.WriteLine(botToken);
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] Update update)
		{
			Console.WriteLine($"Bot {await _botClient.GetMeAsync()} is running..."); ;

			try
			{
				var cancellationToken = new CancellationToken();
				await _messageService.HandleUpdateAsync(_botClient, update, cancellationToken);
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
			Console.WriteLine("health - OK");
			return Ok("OK");
		}

		
		#region Test

		public void Test()
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

			Task.Run(async () => await ConfigureWebhookAsync(true));
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

		public async Task ConfigureWebhookAsync(bool local)
		{
			if (local)
			{
				await _botClient.DeleteWebhookAsync();
			}
			else
			{
				var wh = await _botClient.GetWebhookInfoAsync();
				if (wh.IpAddress is null)
				{
					const string webhookUrl = "https://telegramantispambot.onrender.com/bot";
					await _botClient.SetWebhookAsync(webhookUrl);
				}
			}
		}
		#endregion
	}
}
