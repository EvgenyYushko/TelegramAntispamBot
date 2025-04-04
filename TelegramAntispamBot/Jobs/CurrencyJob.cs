using System.Threading.Tasks;
using BuisinessLogic.Services.Parsers;
using Infrastructure.InjectSettings;
using Infrastructure.Models.AI;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.Jobs.Base;

namespace TelegramAntispamBot.Jobs
{
	public class CurrencyJob : SchedulerJob
	{
		private readonly NbrbCurrencyParser _nbrbCurrencyParser;

		public CurrencyJob(TelegramInject botClient, NbrbCurrencyParser nbrbCurrencyParser, ITelegramUserService telegramUserService)
			: base(botClient, telegramUserService)
		{
			_nbrbCurrencyParser = nbrbCurrencyParser;
		}

		/// <inheritdoc />
		protected override Task<string> Parse(ParseParams parseParams)
		{
			return _nbrbCurrencyParser.ParseCurrencyRates();
		}
	}
}