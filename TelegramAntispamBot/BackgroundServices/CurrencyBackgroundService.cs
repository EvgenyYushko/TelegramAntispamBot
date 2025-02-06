using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuisinessLogic.Services.Parsers;
using Infrastructure.InjectSettings;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TelegramAntispamBot.BackgroundServices
{
	public class CurrencyBackgroundService : BackgroundService
	{
		private readonly NbrbCurrencyParser _nbrbCurrencyParser;
		private readonly TelegramBotClient _telegramClient;
		private Timer _timer;

		public CurrencyBackgroundService(TelegramInject botClient, NbrbCurrencyParser nbrbCurrencyParser)
		{
			_nbrbCurrencyParser = nbrbCurrencyParser;
			_telegramClient = botClient.TelegramClient;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var now = DateTime.Now;
			var nextRun = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);

			if (now > nextRun)
			{
				nextRun = nextRun.AddDays(1);
			}

			var delay = nextRun - now;

			_timer = new Timer(DoWork, null, delay, TimeSpan.FromDays(1));

			return Task.CompletedTask;
		}

		private void DoWork(object state)
		{
			try
			{
				var currStr = _nbrbCurrencyParser.ParseCurrencyRates(DateTime.Now).Result;
				if (currStr is not null)
				{
					Task.Run(async () =>
					{
						var channelsId = new List<long>()
						{
							-1002227239224, // Тест бота
							-1002360730808  // Женя тестирует бота
						};

						var tasks = channelsId
							.Select(channelId => _telegramClient.SendTextMessageAsync(channelId, currStr, parseMode: ParseMode.Markdown))
							.Cast<Task>().ToArray();

						await Task.WhenAll(tasks);

					}).Wait();
					return;
				}

				Console.WriteLine("Пустые данные валют");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Telegram error: {ex.Message}");
			}
		}

		public override async Task StopAsync(CancellationToken stoppingToken)
		{
			_timer?.Change(Timeout.Infinite, 0);
			await base.StopAsync(stoppingToken);
		}
	}
}
