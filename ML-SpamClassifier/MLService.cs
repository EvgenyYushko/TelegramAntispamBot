using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DomainLayer.Models;
using DomainLayer.Repositories;
using GoogleServices.Interfaces;
using ServiceLayer.Models;
using ServiceLayer.Services.Telegram;
using static ML_SpamClassifier.Helpers.MLHelpers;

namespace ML_SpamClassifier
{
	public class MLService : IMLService
	{
		private readonly ITelegramUserRepository _telegramUserRepository;
		private readonly IGoogleDriveUploader _googleDriveUploader;

		public MLService(ITelegramUserRepository telegramUserRepository, IGoogleDriveUploader googleDriveUploader)
		{
			_telegramUserRepository = telegramUserRepository;
			_googleDriveUploader = googleDriveUploader;
		}

		public Task AddSuspiciousMessages(SuspiciousMessageDto message)
		{
			return _telegramUserRepository.AddSuspiciousMessages(new SuspiciousMessage()
			{
				Text = message.Text,
				CreatedAt = message.CreatedAt,
				IsSpamByGemini = message.IsSpamByGemini,
				IsSpamByMl = message.IsSpamByMl,
				IsSpamByUser = message.IsSpamByUser,
				NeedsManualReview = message.NeedsManualReview,
				Probability = message.Probability
			});
		}

		public Task UpdateSuspiciousMessages(SuspiciousMessageDto message)
		{
			return _telegramUserRepository.UpdateSuspiciousMessages(new SuspiciousMessage()
			{
				Id = message.Id,
				Text = message.Text,
				CreatedAt = message.CreatedAt,
				IsSpamByGemini = message.IsSpamByGemini,
				IsSpamByMl = message.IsSpamByMl,
				IsSpamByUser = message.IsSpamByUser,
				NeedsManualReview = message.NeedsManualReview,
				Probability = message.Probability
			});
		}

		public List<SuspiciousMessageDto> GetAllSuspiciousMessages()
		{
			var msgs = _telegramUserRepository.GetAllSuspiciousMessages();

			return msgs.Select(m => new SuspiciousMessageDto()
			{
				Text = m.Text,
				IsSpamByUser = m.IsSpamByUser,
				NeedsManualReview = m.NeedsManualReview,
				Probability = m.Probability,
				CreatedAt = m.CreatedAt,
				Id = m.Id,
				IsSpamByGemini = m.IsSpamByGemini,
				IsSpamByMl = m.IsSpamByMl
			}).ToList();
		}

		public Task DeleteReviewedSuspiciousMessages()
		{
			return _telegramUserRepository.DeleteReviewedSuspiciousMessages();
		}

		public async Task DownloadModel()
		{
			// Загрузка сохраненной модели
			var modelFile = await _googleDriveUploader.GetFileByNameAsync(_modelFileName, _googleFolderId);
			if (modelFile != null)
			{
				// Загрузка файла с Google Диска
				await _googleDriveUploader.DownloadFileAsync(modelFile.Id, _modelPath);
				Console.WriteLine(GetModelStatus());
			}
		}

		public async Task UploadModelAndDataSetToDrive()
		{
			await _googleDriveUploader.UploadFileAsync(_modelPath, _googleFolderId, true);
			await _googleDriveUploader.UploadFileAsync(_dataSetPath, _googleFolderId, true);
		}

		public async Task<bool> UpdateDataSet()
		{
			var msgs = _telegramUserRepository.GetAllSuspiciousMessages();
			var msgToUpdate = msgs.Where(m => !m.NeedsManualReview && m.IsSpamByUser is not null);
			if (!msgToUpdate.Any())
			{
				return false;
			}

			var dataSetFile = await _googleDriveUploader.GetFileByNameAsync(_dataSetFileName, _googleFolderId);
			if (dataSetFile is not null)
			{
				await _googleDriveUploader.DownloadFileAsync(dataSetFile.Id, _dataSetPath);
			}

			if (!File.Exists(_dataSetPath))
				throw new FileNotFoundException($"Файл {_dataSetPath} с данными не найден");

			foreach (var msg in msgToUpdate)
			{
				var cleanText = CleanResponse(msg.Text);
				SaveToCsv(cleanText, msg.IsSpamByUser.Value ? 1 : 0);
			}

			return true;
		}

		private void SaveToCsv(string text, int label)
		{
			var cleanText = text.Replace("\"", "'");
			cleanText = cleanText.Replace("\n", " ");

			using (var writer = new StreamWriter(_dataSetPath, true, Encoding.UTF8))
			{
				writer.WriteLine($"{label},{cleanText}");
			}
		}

		static string CleanResponse(string text)
		{
			var patterns = new[]
			{
				"\"",
				"\\*"
			};

			foreach (var pattern in patterns)
			{
				text = Regex.Replace(text, pattern, "", RegexOptions.IgnoreCase);
			}

			return Regex.Replace(text.Trim(), @"\s+", " ");
		}
	}
}
