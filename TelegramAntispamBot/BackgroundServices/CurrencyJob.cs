using System;
using System.Threading.Tasks;
using BuisinessLogic.Services.Parsers;
using Infrastructure.InjectSettings;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.BackgroundServices.Base;

namespace TelegramAntispamBot.BackgroundServices
{
	public class CurrencyJob : SchedulerJob
	{
		private readonly NbrbCurrencyParser _nbrbCurrencyParser;

		public CurrencyJob(TelegramInject botClient, NbrbCurrencyParser nbrbCurrencyParser,
			ITelegramUserService telegramUserService)
			: base(botClient, new BackgroundSiteSetting
				{
					ScheduledTimes = new[]
					{
						new TimeSpan(9, 0, 0) // 09:00
					}
				},
				telegramUserService)
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