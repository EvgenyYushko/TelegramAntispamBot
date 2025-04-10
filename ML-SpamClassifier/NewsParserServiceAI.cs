using System;
using System.Collections.Generic;
using System.Linq;
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
			var response = await _generativeLanguageModel.AskGemini(prompt);

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
				var response = await _generativeLanguageModel.AskGemini(prompt);
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
