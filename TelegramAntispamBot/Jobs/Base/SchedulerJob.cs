using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.InjectSettings;
using Quartz;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TelegramAntispamBot.Jobs.Base
{
	public abstract class SchedulerJob : IJob
	{
		private readonly TelegramBotClient _telegramClient;
		protected readonly ITelegramUserService _telegramUserService;

		protected SchedulerJob(TelegramInject botClient, ITelegramUserService telegramUserService)
		{
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
}