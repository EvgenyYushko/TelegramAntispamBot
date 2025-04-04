using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleServices.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.ML;
using ML_SpamClassifier.Helpers;
using ML_SpamClassifier.Interfaces;
using ML_SpamClassifier.Models;
using ML_SpamClassifier.Models.Gemini;
using ML_SpamClassifier.Models.Gemini.Criteries;
using Newtonsoft.Json;
using ServiceLayer.Models;
using ServiceLayer.Services.Telegram;
using static Infrastructure.Common.TimeZoneHelper;
using static Infrastructure.Helpers.Logger;
using static ML_SpamClassifier.Helpers.MLHelpers;

namespace ML_SpamClassifier
{
	public class SpamDetector : ISpamDetector
	{
		private readonly IGenerativeLanguageModel _generativeLanguageModel;
		private readonly MLContext _mlContext = new();
		private readonly IMLService _msService;
		private readonly IWebHostEnvironment _env;
		private ITransformer _model;
		private PredictionEngine<MessageData, PredictionResult> _predictor;

		public SpamDetector(IGenerativeLanguageModel generativeLanguageModel, IMLService msService, IWebHostEnvironment env)
		{
			_generativeLanguageModel = generativeLanguageModel;
			_msService = msService;
			_env = env;
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
			Log("TrainModelAsync - Start");
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
			Log(GetModelStatus());
			Log("TrainModelAsync - End");
		}

		private void CreatePredictor()
		{
			_predictor = _mlContext.Model.CreatePredictionEngine<MessageData, PredictionResult>(_model);
		}

		public bool IsSpam(string text, string chatTheme, ref string comment)
		{
			if (_env.IsDevelopment())
			{
				return false;
			}

			var prediction = _predictor.Predict(new MessageData { Text = text });

			//if (0.10 < prediction.Probability && prediction.Probability < 0.90) // сохраним для анализа
			//{

			var (isSpamByGemini, details) = Task.Run(async () => await CheckSpamWithScoringAsync(text, chatTheme)).Result;
			if (isSpamByGemini)
			{
				comment = details;
			}

			Log($"IsSpam = {prediction.IsSpam}, Probability = {prediction.Probability}, isSpamByGemini = {isSpamByGemini}");

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

			return isSpamByGemini;
			//}

			//return prediction.IsSpam;
		}

		public async Task<(bool IsSpam, string Details)> CheckSpamWithScoringAsync(string message, string theme)
		{
			var criteria = GetDefaultSpamCriteria(theme);
			message = message.EscapePromptInjection();

			// Быстрая предварительная проверка
			var totalScore = QuickSpamCheck(message, criteria);
			if (totalScore.IsSpam)
			{
				var geminiExplanation = await GetDetailedSpamExplanationAsync(message, theme, criteria);
				return (true, geminiExplanation);
			}

			// Подробный анализ через Gemini
			var prompt = BuildSpamDetectionPrompt(message, theme, criteria);
			var response = await _generativeLanguageModel.AskGemini(prompt);
			var result = ParseGeminiResponse(response);
			if (result.Details.Contains("Ошибка"))
			{
				var jsonStart = response.IndexOf('{');
				var jsonEnd = response.LastIndexOf('}');
				if (jsonStart >= 0 && jsonEnd > jsonStart)
				{
					var json = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
					result = ParseGeminiResponse(json);
					if (result.Details.Contains("Ошибка"))
					{
						return (false, null);
					}
				}
			}

			return (result.IsSpam, result.Details);
		}

		private (bool IsSpam, double Score) QuickSpamCheck(string message, List<SpamCriterion> criteria)
		{
			// Проверка ссылок
			if (ContainsSuspiciousLinks(message))
			{
				return (true, 0);
			}

			double totalScore = 0;
			var lowerMessage = message.ToLower();

			foreach (var criterion in criteria)
			{
				// Проверка по ключевым словам
				if (criterion.Keywords?.Any(kw => lowerMessage.Contains(kw.ToLower())) ?? false)
				{
					totalScore += criterion.Weight;
					continue;
				}
			}

			totalScore = CalculateModifiers(message, totalScore);

			return (totalScore > 1.5, totalScore);
		}

		private static double CalculateModifiers(string message, double totalScore)
		{
			// Эмодзи
			totalScore += CalculateEmojiModifier(message/*, userInfo*/);

			// CAPSLOCK
			var capsCount = message.Count(char.IsUpper);
			if ((double)capsCount / message.Length > 0.3)
				totalScore += 0.3;

			var analyzer = new TextRepetitionAnalyzer();
			totalScore += analyzer.CalculateRepetitionScore(message);

			if (message.Length > 100)
			{
				var lengthFactor = Math.Min(message.Length / 500.0, 1.0);
				totalScore *= lengthFactor;
			}

			//// Повтор сообщения
			//if (_messageHistory.IsMessageRepeated(message, user.Id))
			//	score += 0.3;

			//// Первое сообщение
			//if (_userService.IsFirstMessage(user.Id))
			//	score += 0.15;

			return totalScore;
		}

		private string BuildSpamDetectionPrompt(string message, string theme, List<SpamCriterion> criteria)
		{
			var criteriaText = string.Join("\n",
				criteria.Select((c, i) => $"{i + 1}. {c.Description} (вес:  0 - {c.Weight})"));

			var examples = string.Join("\n",
				new[]
				{
					"- СПАМ: \"ПОЛУЧИ 1000$ НА КАРТУ! https://scam.com\" [критерии: 1,2,3,5]",
					"- СПАМ: \"Инвестируй в крипту - 300% гарантия!\" [критерии: 1,6]",
					"- Не спам: \"Как подключить Gemini API в C#?\""
				});

			return
					"### Задача\n" +
					"Оцени сообщение чата в телеграмм по критериям и верни JSON.\n\n" +

					"### Контекст чата\n" +
				   $"Тематика: \"{theme}\"\n\n" +

				   "### Критерии\n" +
				   $"{criteriaText}\n\n" +

					"Модификаторы:\n" +
					"- Наличие эмодзи: +0.2 за > 3\n" +
					"- Повтор сообщения: +0.3\n" +
					"- CAPSLOCK: +0.3 если >30% текста\n" +

					"### Формула расчета\n" +
					$"total = sum(Критерии) + Модификаторы > 1.5 → спам\n" +

				   "### Важные указания\n" +
				   "1. Если сообщение попадает под несколько критериев - учитывай их суммарный вес\n" +
				   "2. При весе > 1.5 считай сообщение спамом, иначе полагайся на своё решение\n" +
				   "3. Ссылки проверяй особенно тщательно" +
				   "4. Ответ должен содержать ТОЛЬКО валидный JSON\n" +
				   "5. Не включай никакого текста кроме JSON\n" +
				   "6. Все строковые значения должны быть в двойных кавычках\n" +
				   "7. Не используй ```json или другие маркеры\n" +

				   "### Анализ вложений\n" +
					"Если сообщение содержит вложения:\n" +
					"- Проверь расширения файлов (опасные: .exe, .scr)\n" +
					"- Ищи упоминания файлов в тексте\n" +
					"- Анализируй подписи к изображениям\n\n" +

					"### Лингвистические маркеры\n" +
					"- Избыточная пунктуация (более 3 ! или ?)\n" +
					"- Смешанные языки в одном сообщении\n" +
					"- Шаблонные фразы типа 'Уникальная возможность' и тд\n\n" +

				   "### Примеры\n" +
				   $"{examples}\n\n" +

				   "### Анализируемое сообщение\n" +
				   $"\"{message}\"\n\n" +

					"### Ответ в формате JSON:\n" +
				   "{\n" +
				   "  \"analysis\": {\n" +
				   "    \"criteria\": [\n" +
				   "      { \"id\": 1, \"score\": 0.9, \"reason\": \"...\" },\n" +
				   "      ...\n" +
				   "    ],\n" +
				   "    \"modifiers\": [\n" +
				   "      { \"type\": \"emojis\", \"score\": 0.2 }\n" +
				   "    ],\n" +
				   "    \"total\": 2.1,\n" +
				   "    \"is_spam\": true\n" +
				   "  }\n" +
				   "}";
		}

		private async Task<string> GetDetailedSpamExplanationAsync(string message, string theme, List<SpamCriterion> criteria)
		{
			var criteriaList = string.Join("\n",
				criteria.Select(c => $"- {c.Description} (вес: {c.Weight})"));

			var prompt = "### Задача\n" +
						 "Дай краткое объяснение по критериям с указанием весов.\n\n" +

						 "### Сообщение\n" +
						 $"\"{message}\"\n\n" +

						 "### Критерии\n" +
						 $"{criteriaList}\n\n" +

						 "Модификаторы:\n" +
						"- Наличие эмодзи: +0.2 за > 3\n" +
						"- Повтор сообщения: +0.3\n" +
						"- CAPSLOCK: +0.3 если >30% текста\n" +

						 "### Формат ответа\n" +
						 "Пример:\n" +
						 "\"Спам (общий вес не менее: 2.3):\n" +
						 "1. Коммерческое предложение (0.9)\n" +
						 "2. Подозрительная ссылка (1.0)\n" +
						 "3. Призыв к действию (0.4)\"\n\n" +

						 "### Лингвистические маркеры\n" +
						"- Избыточная пунктуация (более 3 ! или ?)\n" +
						"- Смешанные языки в одном сообщении\n" +
						"- Шаблонные фразы типа 'Уникальная возможность' и тд\n\n" +

						 "Твой анализ:\n" +
						 "#Постарайся отвечать как можно кратко. По существу и без лишней воды.";

			return await _generativeLanguageModel.AskGemini(prompt);
		}

		private (bool IsSpam, string Details) ParseGeminiResponse(string jsonResponse)
		{
			try
			{
				// Удаляем возможные форматирующие символы в начале/конце
				jsonResponse = jsonResponse.Trim('`', ' ', '\n', '\r', '\t');

				// Удаляем маркер "json" если есть
				if (jsonResponse.StartsWith("json", StringComparison.OrdinalIgnoreCase))
				{
					jsonResponse = jsonResponse.Substring(4).TrimStart();
				}

				// Десериализация с обработкой ошибок
				var response = JsonConvert.DeserializeObject<GeminiAnalysisResponse>(jsonResponse, new JsonSerializerSettings
				{
					Error = (sender, args) =>
					{
						args.ErrorContext.Handled = true;
						Log($"JSON parse error: {args.ErrorContext.Error.Message}");
					},
					MissingMemberHandling = MissingMemberHandling.Ignore
				});

				if (response?.Analysis == null)
				{
					Log("Invalid response format or empty analysis");
					return (false, "Ошибка анализа: неверный формат ответа");
				}

				var details = new StringBuilder();
				details.AppendLine("Детализация оценки спама:");

				foreach (var criterion in response.Analysis.Criteria.Where(c => c.Score > 0))
				{
					details.AppendLine($"- Критерий {criterion.Id}: {criterion.Score:0.##} балла");
					details.AppendLine($"  Причина: {criterion.Reason}");
				}

				foreach (var modifier in response.Analysis.Modifiers.Where(m => m.Score > 0))
				{
					details.AppendLine($"- Модификатор '{modifier.Type}': {modifier.Score:0.##} балла");
					if (!string.IsNullOrEmpty(modifier.Reason))
						details.AppendLine($"  Причина: {modifier.Reason}");
				}

				details.AppendLine($"\nИтоговый балл: {response.Analysis.Total:0.##} (порог: 1.5)");
				details.AppendLine($"Решение: {(response.Analysis.IsSpam ? "СПАМ" : "Не спам")}");

				return (response.Analysis.IsSpam, details.ToString());
			}
			catch (Exception ex)
			{
				Log(ex);
				return (false, $"Ошибка при анализе ответа: {ex.Message}");
			}
		}

		private static List<SpamCriterion> GetDefaultSpamCriteria(string chatTheme)
		{
			return new List<SpamCriterion>
			{
				new() {
					Id = "C1",
					Description = "Коммерческие предложения вне контекста",
					Weight = 0.9,
					Keywords = new[] { "купите", "акция", "скидка", "только сегодня", "предложение" }
				},
				new() {
					Id = "C2",
					Description = "Подозрительные ссылки",
					Weight = 1.0, // Максимальный вес
					CheckLinks = true
				},
				new() {
					Id = "C3",
					Description = "Призывы к срочным действиям",
					Weight = 0.7,
					Keywords = new[] { "срочно", "быстро", "успей", "последний шанс" }
				},
				new() {
					Id = "C4",
					Description = $"Несоответствие тематике \"{chatTheme}\"",
					Weight = 0.6,
					Keywords = Array.Empty<string>() // Специфично для чата
				},
				new() {
					Id = "C5",
					Description = "Грамматические аномалии",
					Weight = 0.4,
					Keywords = new[] { "!!!!!!", "?????", "ВСЕМ СРОЧНО" }
				},
				new() {
					Id = "C6",
					Description = "Финансовые схемы",
					Weight = 0.8,
					Keywords = new[] { "заработок", "гарантия", "прибыль", "криптовалюта" }
				}
			};
		}
	}
}