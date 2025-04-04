﻿using System;
using System.Threading.Tasks;
using ML_SpamClassifier.Interfaces;
using ServiceLayer.Services.Telegram;
using static Infrastructure.Helpers.Logger;

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
			Log("MLFacade.Load - Start");
			await _mLService.DownloadModel();
			await _spamDetector.LoadModel();
			Log("MLFacade.Load - End");
		}

		public async Task<bool> RetrainModel()
		{
			Log("RetrainModel - Start");

			var isUpdated = await _mLService.UpdateDataSet();
			if (isUpdated)
			{
				await _spamDetector.TrainModelAsync();
				await _mLService.UploadModelAndDataSetToDrive();
				await _mLService.DeleteReviewedSuspiciousMessages();
			}

			Log("RetrainModel - End");

			return isUpdated;
		}
	}
}
