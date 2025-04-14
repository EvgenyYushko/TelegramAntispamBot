using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using GoogleServices.Interfaces;
using Newtonsoft.Json.Linq;
using ServiceLayer.Models.News;
using ServiceLayer.Services.AI;
using static Infrastructure.Helpers.Logger;

namespace ML_SpamClassifier
{
	public class NewsParserServiceAI : INewsParserServiceAI
	{
		private readonly IGenerativeLanguageModel _generativeLanguageModel;

		public NewsParserServiceAI(IGenerativeLanguageModel generativeLanguageModel)
		{
			_generativeLanguageModel = generativeLanguageModel;
		}

		private List<RssFeed> _availableFeeds = new List<RssFeed>();

		public async Task<RssFeed> SelectMostRelevantFeedAsync(string chatTitle, string chatDescription, List<string> lastMessages, List<RssFeed> availableFeeds)
		{
			_availableFeeds = availableFeeds;

			// Формируем список доступных RSS-лент для промта
			var feedsList = string.Join("\n", availableFeeds.Select(f =>
				$"- {f.Name} ({f.Category}): {f.Description}"));

			// Создаем промт для Gemini
			var prompt = "Анализ Telegram-чата для выбора RSS-ленты:\n\n" +
				"### Контекст чата:\n" +
				"- Название: " + chatTitle + "\n" +
				"- Описание: " + (chatDescription ?? "нет описания") + "\n" +
				"- Последние сообщения:\n" +
				(lastMessages is null ? "нету сообщений" : string.Join("\n", lastMessages.TakeLast(10).Select((m, i) => $"  {i + 1}. {m}")))
				+ "\n\n" +

				"### Доступные RSS-ленты (используй ТОЧНО эти названия):\n" +
				string.Join("\n", _availableFeeds.Select(f => $"ТОЧНОЕ_НАЗВАНИЕ_НОВОСТИ: \"" + f.Name + "\" (её категория: \"" + f.Category + "\", её описание: \"" + f.Description + ")")) + "\"\n\n" +

				"### Задача:\n" +
				"1. Проанализируй тематику чата\n" +
				"2. Выбери 3 наиболее релевантных ленты ИЗ ПРЕДОСТАВЛЕННОГО СПИСКА\n" +
				"3. Используй ТОЧНЫЕ названия лент как они указаны выше\n\n" +

				"### Формат ответа (строго JSON):\n" +
				"{\n" +
				"  \"reason\": \"анализ тематики и обоснование выбора\",\n" +
				"  \"top_feeds\": [\"ТОЧНОЕ_НАЗВАНИЕ_НОВОСТИ_1\", \"ТОЧНОЕ_НАЗВАНИЕ_НОВОСТИ_2\", \"ТОЧНОЕ_НАЗВАНИЕ_НОВОСТИ_3\"],\n" +
				"  \"match_score\": [0-100] // оценка релевантности для каждого выбора\n" +
				"}";


			// Отправляем запрос к Gemini API
			var response = await _generativeLanguageModel.GeminiRequest(prompt);

			// Парсим ответ и находим выбранные ленты
			return ParseGeminiResponse(response);
		}

		private RssFeed ParseGeminiResponse(string response)
		{
			try
			{
				// Очистка ответа (удаляем markdown обертку если есть)
				var jsonStr = response.Replace("```json", "").Replace("```", "").Trim();

				var json = JObject.Parse(jsonStr);
				var topFeedName = json["top_feeds"]?[0]?.ToString();

				return _availableFeeds.FirstOrDefault(f => f.Name == topFeedName)
					   ?? GetFallbackFeed();
			}
			catch
			{
				return GetFallbackFeed();
			}
		}

		private RssFeed GetFallbackFeed()
		{
			// Возвращаем фид по умолчанию (например, общие новости)
			return _availableFeeds.FirstOrDefault(f => f.Name == "РИА Новости")
				   ?? _availableFeeds[0];
		}

		public async Task<SyndicationItem> GetMostRelevantNewsItemAsync(SyndicationFeed feed, string chatTitle
			, string chatDescription = null
			, List<string> lastMessages = null)
		{
			// 1.Подготовка данных для анализа
			var newsTitles = feed.Items
				.Select(item => item.Title.Text)
				.Take(20) // Ограничиваем количество для анализа
				.ToList();

			// 2. Формируем промпт для Gemini
			var prompt = "Анализ новостей для выбора наиболее релевантной:\n\n" +
				"### Контекст чата:\n" +
				"- Название: " + chatTitle + "\n" +
				"- Описание: " + (string.IsNullOrEmpty(chatDescription) ? "нет описания" : chatDescription) + "\n" +
				"- Последние сообщения: " + (lastMessages != null && lastMessages.Any() ? string.Join("\n", lastMessages.TakeLast(5)) : "нет сообщений") + "\n\n" +

				"### Список новостей (выбери 3 наиболее релевантных):\n" +
				string.Join("\n", newsTitles.Select((t, i) => (i + 1) + ". " + t)) + "\n\n" +

				"### Задача:\n" +
				"1. Проанализируй тематику чата\n" +
				"2. Выбери 3 наиболее релевантных новости из списка\n" +
				"3. Укажи номера выбранных новостей (только цифры)\n\n" +

				"### Формат ответа (строго JSON):\n" +
				"{\n" +
				"  \"reason\": \"анализ тематики и обоснование выбора\",\n" +
				"  \"selected_news\": [1, 3, 5] // номера выбранных новостей\n" +
				"}";

			// 3. Отправляем запрос к Gemini API
			var response = await _generativeLanguageModel.GeminiRequest(prompt);

			// 4. Парсим ответ и выбираем новость
			return ParseGeminiResponse(feed, response);
		}

		private SyndicationItem ParseGeminiResponse(SyndicationFeed feed, string response)
		{
			try
			{
				// Очистка ответа от markdown обертки
				var jsonStr = response.Replace("```json", "").Replace("```", "").Trim();
				var json = JObject.Parse(jsonStr);

				// Получаем индексы выбранных новостей
				var selectedIndices = json["selected_news"]?.ToObject<List<int>>() ?? new List<int>();

				// Берем первую наиболее релевантную новость
				if (selectedIndices.Any())
				{
					// Корректируем индекс (в ответе 1-based)
					var firstIndex = selectedIndices[0] - 1;
					if (firstIndex >= 0 && firstIndex < feed.Items.Count())
					{
						return feed.Items.ElementAt(firstIndex);
					}
				}
			}
			catch (Exception ex)
			{
				Log($"Ошибка парсинга ответа Gemini: {ex.Message}");
			}

			// Fallback: возвращаем первую новость, если не удалось распарсить ответ
			return feed.Items.FirstOrDefault();
		}

		public async Task<List<string>> GetRelevantTagsAsync(string chatTheme, List<string> availableTags)
		{
			try
			{
				// 1. Формируем промпт для Gemini
				string prompt = "Ты - помощник для подбора релевантных тегов из списка.\n" +
							  $"Чат называется \"{chatTheme}\".\n" +
							  "Выбери ТОЧНО 3 наиболее подходящих тега из предложенных, которые лучше всего соответствуют тематике чата.\n" +
							  "Возвращай ТОЛЬКО теги в формате: \"tag1, tag2, tag3\" без дополнительных пояснений.\n\n" +
							  "Доступные теги:\n" +
							  $"{string.Join(", ", availableTags)}\n\n" +
							  "Пример ответа для чата \"IT для женщин\":\n" +
							  "\"Карьера в IT, Women in Tech, Разработка\"";

				// 2. Отправляем запрос к Gemini API
				var response = await _generativeLanguageModel.GeminiRequest(prompt);
				var responseText = response.Trim();

				// 3. Парсим ответ и валидируем результат
				var selectedTags = responseText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
											.Select(t => t.Trim())
											.Where(t => availableTags.Contains(t))
											.Take(3)
											.ToList();

				return selectedTags;
			}
			catch (Exception ex)
			{
				Log("Ошибка при получении тегов от Gemini: " + ex.ToString());
				return new List<string>();
			}
		}
	}
}
