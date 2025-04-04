using System.Threading.Tasks;
using BuisinessLogic.Services.Parsers;
using Infrastructure.InjectSettings;
using Infrastructure.Models.AI;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.Jobs.Base;

namespace TelegramAntispamBot.Jobs
{
	public class HabrJob : SchedulerJob
	{
		private readonly HabrParser _habrParser;

		public HabrJob(TelegramInject botClient, HabrParser habrParser, ITelegramUserService telegramUserService)
			: base(botClient, telegramUserService)
		{
			_habrParser = habrParser;
		}

		/// <inheritdoc />
		protected override Task<string> Parse(ParseParams parseParams)
		{
			return _habrParser.ParseLatestPostAsync(parseParams);
		}
	}
}