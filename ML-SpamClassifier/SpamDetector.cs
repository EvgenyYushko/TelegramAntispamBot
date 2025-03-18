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

		private readonly string _dataPath = "..\\..\\Data\\spam_data.csv";
		private static string _modelFileName = "model.zip";

		private readonly string _modelPath = Path.Combine(AppContext.BaseDirectory, _modelFileName);
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

		public void LoadOrTrainModel()
		{
			if (true/*File.Exists(_modelPath)*/)
			{
				// Загрузка сохраненной модели
				var modelFile = Task.Run(async () => await _googleDriveUploader.GetFileByNameAsync(_modelFileName, _googleFolderId)).Result;
				if (modelFile != null)
				{
					// Загрузка файла с Google Диска
					Task.Run(async () => await _googleDriveUploader.DownloadFileAsync(modelFile.Id, _modelPath)).Wait();

					// Загрузка модели с локального диска
					_model = _mlContext.Model.Load(_modelPath, out _);
				}
			}
			else
			{
				// Обучение новой модели
				TrainModel();
			}
			_predictor = _mlContext.Model.CreatePredictionEngine<MessageData, PredictionResult>(_model);

			Console.WriteLine(GetModelStatus());
		}

		public async Task RetrainModelAsync()
		{
			await Task.Run(() => TrainModel());
			_predictor = _mlContext.Model.CreatePredictionEngine<MessageData, PredictionResult>(_model);
		}

		private void TrainModel()
		{
			if (!File.Exists(_dataPath))
				throw new FileNotFoundException($"Файл {_dataPath} с данными не найден");

			var data = _mlContext.Data.LoadFromTextFile<MessageData>(
				_dataPath,
				hasHeader: true,
				separatorChar: ',');

			var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(MessageData.Text))
				.Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

			_model = pipeline.Fit(data);
			_mlContext.Model.Save(_model, data.Schema, _modelPath);
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
					var geminiResponse = Task.Run(async () => await _generativeLanguageModel.AskGemini($"Почему это сообщение является спамом?: {text}\n" +
						$"Ответ дай в формате HTML")).Result;
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
			var geminiResponse = await _generativeLanguageModel.AskGemini($"Является ли это сообщение спамом? Ответь только 'да' или 'нет': {messageText}\n" +
				$"");

			return geminiResponse.ToLower().Contains("да");
		}

		public void AddSpamSample(string text) => AddSample(text, true);
		public void AddHamSample(string text) => AddSample(text, false);

		private void AddSample(string text, bool isSpam)
		{
			var line = $"{isSpam},\"{text.Replace("\"", "\"\"")}\"{Environment.NewLine}";
			using (var writer = new StreamWriter(_dataPath, true, Encoding.UTF8))
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
