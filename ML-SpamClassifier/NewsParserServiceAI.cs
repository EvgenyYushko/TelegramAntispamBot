using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleServices.Interfaces;
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
