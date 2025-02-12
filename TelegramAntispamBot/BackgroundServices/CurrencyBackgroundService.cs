using System;
using System.Threading.Tasks;
using BuisinessLogic.Services.Parsers;
using Infrastructure.InjectSettings;
using TelegramAntispamBot.BackgroundServices.Base;

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
					new TimeSpan(16, 8, 0), // 09:00
				}
			})
		{
			_nbrbCurrencyParser = nbrbCurrencyParser;
		}

		/// <inheritdoc />
		protected override Task<string> Parse()
		{
			return _nbrbCurrencyParser.ParseCurrencyRates();
		}
	}
}
