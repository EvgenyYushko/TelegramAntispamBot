using System;
using System.Threading.Tasks;
using BuisinessLogic.Services.Parsers;
using Infrastructure.InjectSettings;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.BackgroundServices.Base;

namespace TelegramAntispamBot.BackgroundServices
{
	public class OnlinerJob : SchedulerJob
	{
		private readonly OnlinerParser _onlinerParser;

		public OnlinerJob(TelegramInject botClient, OnlinerParser onlinerParser,
			ITelegramUserService telegramUserService)
			: base(botClient, new BackgroundSiteSetting
				{
					ScheduledTimes = new[]
					{
						new TimeSpan(13, 0, 0) // 13:00
					}
				},
				telegramUserService)
		{
			_onlinerParser = onlinerParser;
		}

		/// <inheritdoc />
		protected override Task<string> Parse()
		{
			return _onlinerParser.ParseLatestPostAsync();
		}
	}
}