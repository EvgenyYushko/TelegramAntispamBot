using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleServices.Interfaces;
using Microsoft.ML;
using ML_SpamClassifier.Interfaces;
using ML_SpamClassifier.Models;
using ServiceLayer.Services.Telegram;
using static Infrastructure.Common.TimeZoneHelper;

namespace ML_SpamClassifier
{
	public class SpamDetector : ISpamDetector
	{
		public string _googleFolderId = "1CoUNOXOUgq9S2jv1YkJXfcUf-oIVstC8";

		private static string _modelFileName = "model.zip";
		private static string _dataSetFileName = "spam_dataset.csv";

		private readonly string _modelPath = Path.Combine(AppContext.BaseDirectory, _modelFileName);
		private readonly string _dataSetPath = Path.Combine(AppContext.BaseDirectory, _dataSetFileName);
		private readonly MLContext _mlContext = new();
		private readonly IGoogleDriveUploader _googleDriveUploader;
		private readonly IGenerativeLanguageModel _generativeLanguageModel;
		private readonly ITelegramUserService _telegramUserService;
		private ITransformer _model;
		private PredictionEngine<MessageData, PredictionResult> _predictor;

		public SpamDetector(IGoogleDriveUploader googleDriveUploader
			, IGenerativeLanguageModel generativeLanguageModel
			, ITelegramUserService telegramUserService)
		{
			_googleDriveUploader = googleDriveUploader;
			_generativeLanguageModel = generativeLanguageModel;
			_telegramUserService = telegramUserService;
		}

		public async Task LoadModel()
		{
			// Загрузка сохраненной модели
			var modelFile = await _googleDriveUploader.GetFileByNameAsync(_modelFileName, _googleFolderId);
			if (modelFile != null)
			{
				// Загрузка файла с Google Диска
				await _googleDriveUploader.DownloadFileAsync(modelFile.Id, _modelPath);

				// Загрузка модели с локального диска
				_model = _mlContext.Model.Load(_modelPath, out _);
			}
			
			CreatePredictor();

			Console.WriteLine(GetModelStatus());
		}

		public async Task TrainModelAsync()
		{
			await TrainModel();
			Console.WriteLine(GetModelStatus());
		}

		private void CreatePredictor()
		{
			_predictor = _mlContext.Model.CreatePredictionEngine<MessageData, PredictionResult>(_model);
		}

		private async Task TrainModel()
		{
			//var allFIles = Task.Run(async () => await _googleDriveUploader.GetAllFilesInFolderAsync(_googleFolderId)).Result;
			var dataSetFile = await _googleDriveUploader.GetFileByNameAsync(_dataSetFileName, _googleFolderId);
			if (dataSetFile is not null)
			{
				await _googleDriveUploader.DownloadFileAsync(dataSetFile.Id, _dataSetPath);
			}

			if (!File.Exists(_dataSetPath))
				throw new FileNotFoundException($"Файл {_dataSetPath} с данными не найден");

			var data = _mlContext.Data.LoadFromTextFile<MessageData>(
				_dataSetPath,
				hasHeader: true,
				separatorChar: ',');

			var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(MessageData.Text))
				.Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

			_model = pipeline.Fit(data);
			_mlContext.Model.Save(_model, data.Schema, _modelPath);

			CreatePredictor();
			UploadModelToDrive();
		}

		private async void UploadModelToDrive()
		{
			await _googleDriveUploader.UploadFileAsync(_modelPath, _googleFolderId, true);
		}

		public bool IsSpam(string text, ref string comment)
		{
			var input = new MessageData { Text = text };
			var prediction = _predictor.Predict(input);

			if (prediction.Probability > 0.75)
			{
				Console.WriteLine($"IsSpam? prediction.Probability = {prediction.Probability}");

				var isSpamByGemini = Task.Run(async () => await CheckWithGeminiAsync(text)).Result;
				if (isSpamByGemini)
				{
					var geminiResponse = Task.Run(async () => await _generativeLanguageModel.AskGemini($"Почему это сообщение является спамом?: {text}")).Result;
					comment = geminiResponse;

					return true;
				}

				Task.Run(async () => await _telegramUserService.AddSuspiciousMessages(new ServiceLayer.Models.SuspiciousMessageDto
				{
					Text = text,
					IsSpamByMl = true,
					IsSpamByGemini = false,
					IsSpamByUser = null,
					Probability = prediction.Probability,
					NeedsManualReview = true,
					CreatedAt = DateTimeNow
				})).Wait();

				return false;
			}

			//if (prediction.IsSpam)
			//{
			//	var geminiResponse = Task.Run(async () => await _generativeLanguageModel.AskGemini($"Почему это сообщение является спамом?: {text}")).Result;
			//	comment = geminiResponse;
			//}

			return false;
		}

		private async Task<bool> CheckWithGeminiAsync(string messageText)
		{
			var geminiResponse = await _generativeLanguageModel.AskGemini($"Определи, является ли сообщение спамом. \r\n" +
				$"Критерии спама:\r\n" +
				$"- Коммерческое предложение\r\n" +
				$"- Призыв к действию ('Срочно')\r\n" +
				$"- Подозрительные ссылки\r\n" +
				$"- Избыточная эмоциональность\r\n" +
				$"Ответь только 'да' или 'нет': {messageText}");

			return geminiResponse.ToLower().Contains("да");
		}

		public void AddSpamSample(string text) => AddSample(text, true);
		public void AddHamSample(string text) => AddSample(text, false);

		private void AddSample(string text, bool isSpam)
		{
			var line = $"{isSpam},\"{text.Replace("\"", "\"\"")}\"{Environment.NewLine}";
			using (var writer = new StreamWriter(_dataSetFileName, true, Encoding.UTF8))
			{
				writer.Write(line);
			}
		}

		public string GetModelStatus()
		{
			var info = new FileInfo(_modelPath);
			return $"Модель обучена: {info.Exists}\n" +
				   $"Размер модели: {info.Length / 1024} KB\n" +
				   $"Последнее обновление: {info.LastWriteTime}";
		}
	}
}
