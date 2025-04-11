using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.InjectSettings;
using Infrastructure.Models.AI;
using Quartz;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using static Infrastructure.Helpers.Logger;
using JobHelper = TelegramAntispamBot.Jobs.Helpers.JobHelper;

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
			var chatIdStr = context.MergedJobDataMap.GetString(JobHelper.ChatId);
			if (!long.TryParse(chatIdStr, out var chatId))
			{
				throw new ArgumentException("chatId not exists");
			}

			try
			{
				var chat = _telegramUserService.GetChatById(chatId);
				var content = await Parse(new ParseParams 
				{ 
					ChatTitle = chat.Title
					, ChatDescription = null
					, lastMessages = chat.ChatMessages
						.OrderByDescending(m => m.CreatedAt)
						.Select(m => m.Text)
						.Take(10)
						.ToList()
				});

				if (content != null)
				{
					await _telegramClient.SendTextMessageAsync(
						chat.TelegramChatId,
						content,
						parseMode: ParseMode.Markdown);
				}
			}
			catch (Exception ex)
			{
				Log($"Error processing chat: {ex}");
			}
		}

		protected virtual Task<string> Parse(ParseParams parseParams)
		{
			throw new NotImplementedException();
		}
	}
}