using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GoogleServices.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.ML;
using ML_SpamClassifier.Interfaces;
using ML_SpamClassifier.Models;
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

			var criteria = GetDefaultSpamCriteria(chatTheme);
			var (isSpamByGemini, score) = Task.Run(async () => await CheckSpamWithScoringAsync(text, chatTheme, criteria)).Result;
			if (isSpamByGemini)
			{
				var geminiExplanation = Task.Run(async () => await GetDetailedSpamExplanationAsync(text, chatTheme, criteria)).Result;
				comment = geminiExplanation;
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


		public async Task<(bool IsSpam, double SpamScore)> CheckSpamWithScoringAsync(string message, string theme, List<SpamCriterion> criteria)
		{
			// Быстрая предварительная проверка
			var quickCheck = QuickSpamCheck(message, criteria);
			if (quickCheck.IsSpam)
				return (true, quickCheck.Score);

			// Подробный анализ через Gemini
			var prompt = BuildSpamDetectionPrompt(message, theme, criteria);
			var response = await _generativeLanguageModel.AskGemini(prompt);

			return (ParseGeminiResponse(response), quickCheck.Score);
		}

		private (bool IsSpam, double Score) QuickSpamCheck(string message, List<SpamCriterion> criteria)
		{
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

				// Проверка ссылок (если требуется)
				if (criterion.CheckLinks && ContainsSuspiciousLinks(message))
				{
					totalScore += 1.6/*criterion.Weight*/;
					break;
				}
			}

			return (totalScore > 1.5, totalScore);
		}

		private bool ContainsSuspiciousLinks(string message)
		{
			var urls = ExtractAllUrls(message);

			// Домены для проверки
			// Популярные сокращатели ссылок
			var urlShorteners = new[]
			{
				"bit.ly", "t.co", "tinyurl.com", "goo.gl", "ow.ly", "buff.ly",
				"adf.ly", "shorte.st", "cutt.ly", "bit.do", "cli.re", "is.gd",
				"v.gd", "bc.vc", "ouo.io", "zzb.bz", "shrink.me", "link.tl",
				"click.ru", "shorturl.at", "tiny.cc", "rb.gy", "soo.gd", "ity.im"
			};

			//Фишинговые/мошеннические домены
			var phishingDomains = new[]
			{
				"paypal-verify.com", "appleid-verify.net", "steamcommunity.ru",
				"facebook-security.xyz", "whatsapp-activate.com", "binance-support.org",
				"amazon-refund.pro", "microsoft-update.live", "instagram-help.xyz"
			};

			// Домены для обхода блокировок
			var bypassDomains = new[]
			{
				"xn--90ais", "xn--p1ai", "xn--80aesf", "xn--80asehdb", // IDN-домены
				"top", "gq", "ml", "cf", "tk", "ga", // Бесплатные доменные зоны
				"xyz", "online", "live", "site", "space", "webcam"
			};

			// Взрослый контент/порно-спам
			var adultDomains = new[]
			{
				"dating24.ru", "flirt4free.com", "webcamteens.xyz",
				"hotgirls.live", "cam4.com", "myfreecams.com",
				"brazzers.com", "pornhub.com", "xvideos.com"
			};

			// Крипто-мошенничество
			var cryptoScam = new[]
			{
				"binance-airdrop.com", "eth-giveaway.xyz", "free-bitcoin.pro",
				"coinbase-rewards.com", "tether-free.io", "walletconnect-scam.com"
			};

			// Вредоносные/эксплойт домены
			var malwareDomains = new[]
			{
				"exploit.in", "malware-distribution.com", "ransomware-decrypt.xyz",
				"virus-download.net", "trojan-horse.pro", "keylogger.space"
			};
			// Домены для обмана (scam)
			var scamDomains = new[]
			{
				"free-iphone15.ru", "win-prize-now.com", "million-dollar-giveaway.xyz",
				"you-won-gift.com", "free-gift-cards.pro", "job-from-home-999k.com"
			};

			var suspiciousDomains = urlShorteners
				.Concat(phishingDomains)
				.Concat(bypassDomains)
				.Concat(adultDomains)
				.Concat(cryptoScam)
				.Concat(malwareDomains)
				.Concat(scamDomains)
				.Distinct()
				.ToArray();

			foreach (var url in urls)
			{
				if (suspiciousDomains.Any(d =>
					url.Contains($".{d}") || // Поддомены
					url.Contains($"{d}/"))  // Прямое совпадение
					)
				{
					return true;
				}
			}

			return false;
		}

		private List<string> ExtractAllUrls(string message)
		{
			// Улучшенное регулярное выражение для всех типов ссылок
			var urlRegex = new Regex(@"
        (?:                         # Несохраняющая группа
            https?://               # http:// или https://
            |                       # ИЛИ
            ftp://                 # ftp://
            |                       # ИЛИ
            www\.                   # www.
            |                       # ИЛИ
            [a-z0-9-]+\.(?:[a-z]{2,}|[a-z]{2}\.[a-z]{2})/  # Домен с путем
            |                       # ИЛИ
            [a-z0-9-]+\.(?:[a-z]{2,}|[a-z]{2}\.[a-z]{2})\b # Просто домен
            |                       # ИЛИ
            \b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b         # IPv4
            |                       # ИЛИ
            \[[a-f0-9:]+\]          # IPv6 в квадратных скобках
        )
        (?:                         # Необязательные части URL
            /[^\s]*                 # Путь
            |                       # ИЛИ
            \?[^\s]*               # Query-параметры
            |                       # ИЛИ
            \#[^\s]*               # Якорь
        )?",
				RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

			var matches = urlRegex.Matches(message);

			// Нормализация найденных URL
			return matches.Select(m =>
			{
				var url = m.Value;
				// Добавляем http:// если нужно
				if (!url.StartsWith("http") && !url.StartsWith("ftp") && !url.StartsWith("www"))
				{
					return "http://" + url;
				}
				return url;
			})
			.Distinct()
			.ToList();
		}

		public string BuildSpamDetectionPrompt(string message, string theme, List<SpamCriterion> criteria)
		{
			var criteriaText = string.Join("\n",
				criteria.Select((c, i) => $"{i + 1}. {c.Description} (вес: {c.Weight})"));

			var examples = string.Join("\n",
				new[]
				{
					"- СПАМ: \"ПОЛУЧИ 1000$ НА КАРТУ! https://scam.com\" [критерии: 1,2,3,5]",
					"- СПАМ: \"Инвестируй в крипту - 300% гарантия!\" [критерии: 1,6]",
					"- Не спам: \"Как подключить Gemini API в C#?\""
				});

			return "### Контекст чата\n" +
				   $"Тематика: \"{theme}\"\n\n" +

				   "### Критерии спама (с весами)\n" +
				   $"{criteriaText}\n\n" +

				   "### Важные указания\n" +
				   "1. Если сообщение попадает под несколько критериев - учитывай их суммарный вес\n" +
				   "2. При весе > 1.5 считай сообщение спамом, иначе полагайся на своё решение\n" +
				   "3. Ссылки проверяй особенно тщательно\n\n" +

				   "### Примеры\n" +
				   $"{examples}\n\n" +

				   "### Анализируемое сообщение\n" +
				   $"\"{message}\"\n\n" +

				   "### Формат ответа\n" +
				   "Только \"да\" или \"нет\" без пояснений";
		}

		public async Task<string> GetDetailedSpamExplanationAsync(string message, string theme, List<SpamCriterion> criteria)
		{
			var criteriaList = string.Join("\n",
				criteria.Select(c => $"- {c.Description} (вес: {c.Weight})"));

			var prompt = "### Задача\n" +
						 "Дай развернутое объяснение по критериям с указанием весов.\n\n" +

						 "### Сообщение\n" +
						 $"\"{message}\"\n\n" +

						 "### Критерии\n" +
						 $"{criteriaList}\n\n" +

						 "### Формат ответа\n" +
						 "Пример:\n" +
						 "\"Спам (общий вес не менее: 2.3):\n" +
						 "1. Коммерческое предложение (0.9)\n" +
						 "2. Подозрительная ссылка (1.0)\n" +
						 "3. Призыв к действию (0.4)\"\n\n" +

						 "Твой анализ:\n" +
						 "#Постарайся отвечать как можно кратко. По существу и без лишней воды.";

			return await _generativeLanguageModel.AskGemini(prompt);
		}

		private bool ParseGeminiResponse(string response)
		{
			if (string.IsNullOrWhiteSpace(response))
				return false; // Пустой ответ → не спам

			// Нормализация строки (удаляем всё, кроме букв/цифр)
			var normalized = Regex.Replace(response, @"[^\w\d]", "").ToLower();

			// Проверка на спам через regex
			if (Regex.IsMatch(normalized, @"^(да|yes|spam|1|true|y|д)$"))
				return true;

			// Проверка на НЕ спам
			if (Regex.IsMatch(normalized, @"^(нет|no|ham|0|false|n|н)$"))
				return false;

			// Анализ по первым 3 символам (для ответов типа "Да, это спам")
			var firstChars = normalized[..Math.Min(3, normalized.Length)];
			return firstChars switch
			{
				"да" or "yes" or "spa" => true,
				"нет" or "no" or "ham" => false,
				_ => false // По умолчанию считаем не спамом
			};
		}

		public class SpamCriterion
		{
			public string Description { get; set; }
			public double Weight { get; set; } // Важность критерия (0.1 - 1.0)
			public string[] Keywords { get; set; } // Ключевые фразы для быстрой проверки
			public bool CheckLinks { get; set; } // Требуется ли проверка ссылок
		}

		// Пример инициализации критериев
		public static List<SpamCriterion> GetDefaultSpamCriteria(string chatTheme)
		{
			return new List<SpamCriterion>
			{
				new() {
					Description = "Коммерческие предложения вне контекста",
					Weight = 0.9,
					Keywords = new[] { "купите", "акция", "скидка", "только сегодня", "предложение" }
				},
				new() {
					Description = "Подозрительные ссылки",
					Weight = 1.0, // Максимальный вес
					CheckLinks = true
				},
				new() {
					Description = "Призывы к срочным действиям",
					Weight = 0.7,
					Keywords = new[] { "срочно", "быстро", "успей", "последний шанс" }
				},
				new() {
					Description = $"Несоответствие тематике \"{chatTheme}\"",
					Weight = 0.6,
					Keywords = Array.Empty<string>() // Специфично для чата
				},
				new() {
					Description = "Грамматические аномалии",
					Weight = 0.4,
					Keywords = new[] { "!!!!!!", "?????", "ВСЕМ СРОЧНО" }
				},
				new() {
					Description = "Финансовые схемы",
					Weight = 0.8,
					Keywords = new[] { "заработок", "гарантия", "прибыль", "криптовалюта" }
				}
			};
		}
	}
}