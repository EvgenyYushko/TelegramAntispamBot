using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.InjectSettings;
using Microsoft.Extensions.Hosting;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using static Infrastructure.Common.TimeZoneHelper;

namespace TelegramAntispamBot.BackgroundServices.Base
{
	public abstract class ShedullerBackgroundService : BackgroundService
	{
		private readonly BackgroundSiteSetting _setting;
		private readonly TelegramBotClient _telegramClient;
		private readonly ITelegramUserService _telegramUserService;
		private Timer _timer;

		protected ShedullerBackgroundService(TelegramInject botClient, BackgroundSiteSetting setting,
			ITelegramUserService telegramUserService)
		{
			_setting = setting;
			_telegramUserService = telegramUserService;
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
					.Select(time => CalculateNextRunDate(now, time, _setting.DayInterval))
					.Where(time => time > now)
					.DefaultIfEmpty(CalculateNextRunDate(now.AddDays(1), scheduledTimes[0], _setting.DayInterval))
					.Min();

				var res = nextRunTimes - now;
				Console.WriteLine($"Next run in: {res.ToString("c")} ({this.GetType().FullName})");
				return res;
			}

			// Запускаем таймер с пересчетом времени при каждом срабатывании
			async void Callback(object _)
			{
				await DoWork();
				_timer.Change(GetNextDelay(), Timeout.InfiniteTimeSpan); // Перезапускаем таймер
			}

			DateTime CalculateNextRunDate(DateTime referenceDate, TimeSpan time, int dayInterval)
			{
				// Если сегодня подходящий день и время еще не прошло
				if (IsScheduledDay(referenceDate, dayInterval) && referenceDate.TimeOfDay < time)
				{
					return referenceDate.Date.Add(time);
				}
    
				// Ищем следующий подходящий день
				var daysToAdd = dayInterval - (referenceDate.DayOfYear % dayInterval);
				return referenceDate.Date.AddDays(daysToAdd).Add(time);
			}

			bool IsScheduledDay(DateTime date, int dayInterval)
			{
				return (date.DayOfYear % dayInterval) == 0;
			}
		}

		protected async virtual Task DoWork()
		{
			try
			{
				var currStr = await Parse();
				if (currStr is not null)
				{
					Console.WriteLine("Parse.Start");

					var allChats = _telegramUserService.GetAllChats();

					var tasks = allChats
						.Where(c => c.ChatPermission.SendNews)
						.Select(channel => _telegramClient.SendTextMessageAsync(channel.TelegramChatId, currStr,
								parseMode: ParseMode.Markdown))
						.Cast<Task>()
						.ToArray();

					Console.WriteLine($"Parse.tasks.Count() - {tasks.Count()}");

					await Task.WhenAll(tasks);

					return;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Telegram error: {ex.Message}");
			}
		}

		protected virtual Task<string> Parse()
		{
			throw new NotImplementedException();
		}

		public override async Task StopAsync(CancellationToken stoppingToken)
		{
			_timer?.Change(Timeout.Infinite, 0);
			await base.StopAsync(stoppingToken);
		}
	}

	public class BackgroundSiteSetting
	{
		public TimeSpan[] ScheduledTimes { get; set; }
		public int DayInterval { get; set; } = 1; // По умолчанию ежедневно
	}
}