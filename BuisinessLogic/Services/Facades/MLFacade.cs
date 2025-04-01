using System;
using System.Threading.Tasks;
using ML_SpamClassifier.Interfaces;
using ServiceLayer.Services.Telegram;

namespace BuisinessLogic.Services.Facades
{
	public class MLFacade
	{
		private readonly IMLService _mLService;
		private readonly ISpamDetector _spamDetector;

		public MLFacade(IMLService mLService, ISpamDetector spamDetector)
		{
			_mLService = mLService;
			_spamDetector = spamDetector;
		}

		public async Task LoadModel()
		{
			Console.WriteLine("MLFacade.Load - Start");
			await _mLService.DownloadModel();
			await _spamDetector.LoadModel();
			Console.WriteLine("MLFacade.Load - End");
		}

		public async Task<bool> RetrainModel()
		{
			var isUpdated = await _mLService.UpdateDataSet();
			if (isUpdated)
			{
				await _spamDetector.TrainModelAsync();
				await _mLService.UploadModelAndDataSetToDrive();
				await _mLService.DeleteReviewedSuspiciousMessages();
			}

			return isUpdated;
		}
	}
}
