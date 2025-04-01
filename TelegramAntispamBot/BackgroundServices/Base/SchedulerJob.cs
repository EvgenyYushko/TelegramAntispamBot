using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.InjectSettings;
using Quartz;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TelegramAntispamBot.BackgroundServices.Base
{
	public abstract class SchedulerJob : IJob
	{
		private readonly BackgroundSiteSetting _setting;
		private readonly TelegramBotClient _telegramClient;
		private readonly ITelegramUserService _telegramUserService;
		private Timer _timer;

		protected SchedulerJob(TelegramInject botClient, BackgroundSiteSetting setting,
			ITelegramUserService telegramUserService)
		{
			_setting = setting;
			_telegramUserService = telegramUserService;
			_telegramClient = botClient.TelegramClient;
		}

		public async virtual Task Execute(IJobExecutionContext context)
		{
			try
			{
				var content = await Parse();
				if (content == null)
					return;

				var allChats = _telegramUserService.GetAllChats();
				var tasks = allChats
					.Where(c => c.ChatPermission.SendNews)
					.Select(channel => _telegramClient.SendTextMessageAsync(
						channel.TelegramChatId,
						content,
						parseMode: ParseMode.Markdown))
					.Cast<Task>()
					.ToArray();

				await Task.WhenAll(tasks);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString(), "Job execution failed");
			}
		}

		protected virtual Task<string> Parse()
		{
			throw new NotImplementedException();
		}
	}

	public class BackgroundSiteSetting
	{
		public TimeSpan[] ScheduledTimes { get; set; }
		public int DayInterval { get; set; } = 1; // По умолчанию ежедневно
	}
}