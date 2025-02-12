using System;
using BuisinessLogic.Services.Parsers;
using Infrastructure.InjectSettings;
using TelegramAntispamBot.BackgroundServices.Base;
using static Infrastructure.Common.TimeZoneHelper;

namespace TelegramAntispamBot.BackgroundServices
{
	public class CurrencyBackgroundService : BaseSiteBackgroundService
	{
		private readonly NbrbCurrencyParser _nbrbCurrencyParser;

		public CurrencyBackgroundService(TelegramInject botClient, NbrbCurrencyParser nbrbCurrencyParser)
			: base(botClient, new BackgroundSiteSetting
			{
				ScheduledTimes = new[]
				{
					new TimeSpan(15, 25, 0), // 09:00
				}
			})
		{
			_nbrbCurrencyParser = nbrbCurrencyParser;
		}

		/// <inheritdoc />
		protected override string Parse()
		{
			return _nbrbCurrencyParser.ParseCurrencyRates(DateTimeNow).Result;
		}
	}
}
