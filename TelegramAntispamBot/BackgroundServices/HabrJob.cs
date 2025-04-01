using System.Threading.Tasks;
using BuisinessLogic.Services.Parsers;
using Infrastructure.InjectSettings;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.BackgroundServices.Base;

namespace TelegramAntispamBot.BackgroundServices
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
		protected override Task<string> Parse()
		{
			return _habrParser.ParseLatestPostAsync();
		}
	}
}