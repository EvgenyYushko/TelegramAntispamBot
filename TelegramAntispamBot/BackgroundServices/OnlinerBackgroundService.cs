using System;
using BuisinessLogic.Services.Parsers;
using Infrastructure.InjectSettings;
using TelegramAntispamBot.BackgroundServices.Base;

namespace TelegramAntispamBot.BackgroundServices
{
	public class OnlinerBackgroundService : BaseSiteBackgroundService
	{
		private readonly OnlinerParser _onlinerParser;

		public OnlinerBackgroundService(TelegramInject botClient, OnlinerParser onlinerParser)
			: base(botClient, new BackgroundSiteSetting
			{
				ScheduledTimes = new[]
				{
					new TimeSpan(13, 0, 0), // 13:00
				}
			})
		{
			_onlinerParser = onlinerParser;
		}

		/// <inheritdoc />
		protected override string Parse()
		{
			return _onlinerParser.ParseLatestPostAsync().Result;
		}
	}
}
