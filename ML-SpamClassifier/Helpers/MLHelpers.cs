using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ML_SpamClassifier.Helpers
{
	internal static class MLHelpers
	{
		internal static string _dataSetFileName = "spam_dataset.csv";
		internal static string _dataSetPath = Path.Combine(AppContext.BaseDirectory, _dataSetFileName);
		internal static string _googleFolderId = "1CoUNOXOUgq9S2jv1YkJXfcUf-oIVstC8";
		internal static string _modelFileName = "model.zip";
		internal static string _modelPath = Path.Combine(AppContext.BaseDirectory, _modelFileName);

		public static string GetModelStatus()
		{
			var info = new FileInfo(_modelPath);
			return $"Модель обучена: {info.Exists}\n" +
					$"Размер модели: {info.Length / 1024} KB\n" +
					$"Последнее обновление: {info.LastWriteTime}";
		}

		internal static bool ContainsSuspiciousLinks(string message)
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

		private static List<string> ExtractAllUrls(string message)
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

		internal static double CalculateEmojiModifier(string message/*, UserInfo user*/)
		{
			double score = 0;

			// Эмодзи (проверяем количество)
			// Полный набор паттернов для поиска эмодзи
			var emojiPattern = @"(\u00a9|\u00ae|[\u2000-\u3300]|[\ud83c-\ud83e][\udc00-\udfff])";
   
			var matches = Regex.Matches(message, emojiPattern);
			var emojiCount = matches.Count;

			if (emojiCount == 0)
				return 0;

			// Прогрессивная шкала начисления баллов
			double modifier = 0;

			if (emojiCount >= 10) // Очень много эмодзи
				modifier = 0.5;
			else if (emojiCount >= 5)
				modifier = 0.3;
			else if (emojiCount >= 3)
				modifier = 0.2;
			else if (emojiCount >= 1)
				modifier = 0.1;

			// Дополнительный процентный бонус за концентрацию эмодзи
			double emojiDensity = (double)emojiCount / message.Length;

			if (emojiDensity > 0.3) // Если более 30% текста - эмодзи
				modifier += 0.25;
			else if (emojiDensity > 0.2)
				modifier += 0.15;
			else if (emojiDensity > 0.1)
				modifier += 0.05;

			// Ограничиваем максимальный модификатор
			return Math.Min(modifier, 0.75);
		}

		internal static string EscapePromptInjection(this string input)
		{
			// Экранируем попытки вставки промпта
			return input
				.Replace("###", "[REDACTED]")
				.Replace("```", "[TRIPLE_BACKTICK]")
				.Replace("\"\"\"", "[TRIPLE_QUOTE]");
		}
	}
}