using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.InjectSettings;
using Infrastructure.Models.AI;
using Quartz;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using static Infrastructure.Helpers.Logger;

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
				var allChats = _telegramUserService.GetAllChats()
					.Where(c => c.ChatPermission.SendNews)
					.ToList();

				var exceptions = new ConcurrentQueue<Exception>();

				Parallel.ForEach(allChats, new ParallelOptions { MaxDegreeOfParallelism = 5 }, channel =>
				{
					try
					{
						// Для синхронного выполнения внутри Parallel.ForEach
						var content = Parse(new ParseParams { ChatTitle = channel.Title }).GetAwaiter().GetResult();

						if (content != null)
						{
							_telegramClient.SendTextMessageAsync(
								channel.TelegramChatId,
								content,
								parseMode: ParseMode.Markdown).GetAwaiter().GetResult();
						}
					}
					catch (Exception ex)
					{
						exceptions.Enqueue(ex);
						Log($"Error processing chat {channel.Title}: {ex}");
					}
				});

				// Если были ошибки - бросаем агрегированное исключение
				if (!exceptions.IsEmpty)
				{
					throw new AggregateException(exceptions);
				}
			}
			catch (AggregateException ae)
			{
				foreach (var ex in ae.InnerExceptions)
				{
					Log($"Parallel error: {ex}");
				}
			}
			catch (Exception ex)
			{
				Log($"Global error: {ex}");
			}
		}

		protected virtual Task<string> Parse(ParseParams parseParams)
		{
			throw new NotImplementedException();
		}
	}
}