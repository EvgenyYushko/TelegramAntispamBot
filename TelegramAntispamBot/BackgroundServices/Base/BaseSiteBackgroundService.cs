using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.InjectSettings;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using static Infrastructure.Common.TimeZoneHelper;

namespace TelegramAntispamBot.BackgroundServices.Base
{
	public abstract class BaseSiteBackgroundService : BackgroundService
	{
		private readonly BackgroundSiteSetting _setting;
		private readonly TelegramBotClient _telegramClient;
		private Timer _timer;

		protected BaseSiteBackgroundService(TelegramInject botClient, BackgroundSiteSetting setting)
		{
			_setting = setting;
			_telegramClient = botClient.TelegramClient;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var scheduledTimes = _setting.ScheduledTimes;

			_timer = new Timer(Callback, null, GetNextDelay(), Timeout.InfiniteTimeSpan);
			//Callback(null);
			return Task.CompletedTask;

			// Функция для вычисления времени до следующего запуска
			TimeSpan GetNextDelay()
			{
				var now = DateTimeNow;
				var nextRunTimes = scheduledTimes
					.Select(time => now.Date.Add(time)) // Получаем DateTime для каждого времени сегодня
					.Where(time => time > now)          // Выбираем только будущие времена
					.DefaultIfEmpty(now.Date.AddDays(1).Add(scheduledTimes[0])) // Если все времена прошли, берем первое время завтра
					.Min(); // Берем ближайшее время

				var res = nextRunTimes - now;
				Console.WriteLine("GetNextDelay = " + res.ToString("c"));

				return res;
			}

			// Запускаем таймер с пересчетом времени при каждом срабатывании
			async void Callback(object _)
			{
				await DoWork();
				_timer.Change(GetNextDelay(), Timeout.InfiniteTimeSpan); // Перезапускаем таймер
			}
		}

		private async Task DoWork()
		{
			try
			{
				var currStr = await Parse();
				if (currStr is not null)
				{
					var channelsId = new List<long>
					{
						-1002227239224 // Тест бота
						//,-1002360730808  // Женя тестирует бота
					};

					var tasks = channelsId
						.Select(channelId => _telegramClient.SendTextMessageAsync(channelId, currStr, parseMode: ParseMode.Markdown))
						.Cast<Task>().ToArray();

					await Task.WhenAll(tasks);

					return;
				}

				Console.WriteLine("Пустые данные валют");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Telegram error: {ex.Message}");
			}
		}

		protected abstract Task<string> Parse();

		public override async Task StopAsync(CancellationToken stoppingToken)
		{
			_timer?.Change(Timeout.Infinite, 0);
			await base.StopAsync(stoppingToken);
		}
	}

	public class BackgroundSiteSetting
	{
		public TimeSpan[] ScheduledTimes { get; set; }
	}
}
