using System;
using System.IO;
using System.Threading.Tasks;
using GoogleServices.Interfaces;
using Microsoft.ML;
using ML_SpamClassifier.Interfaces;
using ML_SpamClassifier.Models;
using ServiceLayer.Models;
using ServiceLayer.Services.Telegram;
using static Infrastructure.Common.TimeZoneHelper;
using static ML_SpamClassifier.Helpers.MLHelpers;

namespace ML_SpamClassifier
{
	public class SpamDetector : ISpamDetector
	{
		private readonly IGenerativeLanguageModel _generativeLanguageModel;
		private readonly MLContext _mlContext = new();
		private readonly IMLService _msService;
		private ITransformer _model;
		private PredictionEngine<MessageData, PredictionResult> _predictor;

		public SpamDetector(IGenerativeLanguageModel generativeLanguageModel, IMLService msService)
		{
			_generativeLanguageModel = generativeLanguageModel;
			_msService = msService;
		}

		public async Task LoadModel()
		{
			if (!File.Exists(_modelPath))
			{
				throw new FileNotFoundException($"Файл {_modelPath} с данными не найден");
			}

			// Загрузка модели с локального диска
			_model = _mlContext.Model.Load(_modelPath, out _);

			CreatePredictor();
		}

		public async Task TrainModelAsync()
		{
			if (!File.Exists(_dataSetPath))
			{
				throw new FileNotFoundException($"Файл {_dataSetPath} с данными не найден");
			}

			var data = _mlContext.Data.LoadFromTextFile<MessageData>(
				_dataSetPath,
				hasHeader: true,
				separatorChar: ',');

			var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(MessageData.Text))
				.Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

			_model = pipeline.Fit(data);
			_mlContext.Model.Save(_model, data.Schema, _modelPath);

			CreatePredictor();
			Console.WriteLine(GetModelStatus());
		}

		private void CreatePredictor()
		{
			_predictor = _mlContext.Model.CreatePredictionEngine<MessageData, PredictionResult>(_model);
		}

		public bool IsSpam(string text, ref string comment)
		{
			var prediction = _predictor.Predict(new MessageData { Text = text });

			Console.WriteLine($"IsSpam = {prediction.IsSpam}, Probability = {prediction.Probability}");

			//if (0.10 < prediction.Probability && prediction.Probability < 0.90) // сохраним для анализа
			//{
			var isSpamByGemini = Task.Run(async () => await CheckWithGeminiAsync(text)).Result;
			//if (isSpamByGemini)
			//{
			//	var geminiResponse = Task.Run(async () => await _generativeLanguageModel.AskGemini($"Почему это сообщение является спамом?: {text}")).Result;
			//	comment = geminiResponse;
			//}

			// Если есть раногласия по поводу решения между моделью и Gemini то сохраним на анализ пользаку
			//if (prediction.IsSpam != isSpamByGemini)
			//{
				Task.Run(async () => await _msService.AddSuspiciousMessages(new SuspiciousMessageDto
				{
					Text = text,
					IsSpamByMl = prediction.IsSpam,
					IsSpamByGemini = isSpamByGemini,
					IsSpamByUser = null,
					Probability = prediction.Probability,
					NeedsManualReview = true,
					CreatedAt = DateTimeNow
				})).Wait();
			//}

			return false;
			//}

			//return prediction.IsSpam;
		}

		private async Task<bool> CheckWithGeminiAsync(string messageText)
		{
			var geminiResponse = await _generativeLanguageModel.AskGemini(
				"Определи, является ли сообщение спамом. \r\n" +
				"Критерии спама:\r\n" +
				"- Коммерческое предложение\r\n" +
				"- Призыв к действию ('Срочно')\r\n" +
				"- Подозрительные ссылки\r\n" +
				"- Избыточная эмоциональность\r\n" +
				$"Ответь только 'да' или 'нет': {messageText}");

			return geminiResponse.ToLower().Contains("да");
		}
	}
}