using BuisinessLogic.Services.Parsers;
using System.Threading.Tasks;
using Infrastructure.InjectSettings;
using Infrastructure.Models.AI;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.Jobs.Base;

namespace TelegramAntispamBot.Jobs
{
	public class AllNewsJob : SchedulerJob
	{
		private readonly AllNewsParser _allNewsParser;

		public AllNewsJob(TelegramInject botClient, ITelegramUserService telegramUserService, AllNewsParser allNewsParser) 
			: base(botClient, telegramUserService)
		{
			_allNewsParser = allNewsParser;
		}

		/// <inheritdoc />
		protected override Task<string> Parse(ParseParams parseParams)
		{
			return _allNewsParser.ParseAllNewsRss(parseParams);
		}
	}
}
