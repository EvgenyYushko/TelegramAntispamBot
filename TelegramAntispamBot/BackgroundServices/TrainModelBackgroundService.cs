using System;
using System.Threading.Tasks;
using Infrastructure.InjectSettings;
using ML_SpamClassifier.Interfaces;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.BackgroundServices.Base;

namespace TelegramAntispamBot.BackgroundServices
{
	public class TrainModelBackgroundService : ShedullerBackgroundService
	{
		private readonly IMLService _mLService;
		private readonly ISpamDetector _spamDetector;

		public TrainModelBackgroundService(TelegramInject botClient, ITelegramUserService telegramUserService, IMLService mLService, ISpamDetector spamDetector)
				: base(botClient, new BackgroundSiteSetting
				{
					ScheduledTimes = new[]
					{
						new TimeSpan(21, 59, 60) // 23:59:00
					}
				},
				telegramUserService)
		{
			_mLService = mLService;
			_spamDetector = spamDetector;
		}

		protected async override Task DoWork()
		{
			var isUpdated = await _mLService.UpdateDataSet();
			if (isUpdated)
			{
				await _spamDetector.TrainModelAsync();
				await _mLService.UploadModelAndDataSetToDrive();
				await _mLService.DeleteReviewedSuspiciousMessages();
			}
		}
	}
}
