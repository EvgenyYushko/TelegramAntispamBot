using System.Threading.Tasks;
using BuisinessLogic.Services.Facades;
using Infrastructure.InjectSettings;
using Quartz;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.Jobs.Base;

namespace TelegramAntispamBot.Jobs
{
	public class TrainModeJob : SchedulerJob
	{
		private readonly MLFacade _mLFacade;

		public TrainModeJob(TelegramInject botClient, ITelegramUserService telegramUserService, MLFacade mLFacade)
			: base(botClient, telegramUserService)
		{
			_mLFacade = mLFacade;
		}

		public override async Task Execute(IJobExecutionContext context)
		{
			await _mLFacade.RetrainModel();
		}
	}
}
