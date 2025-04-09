using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleServices.Gemini;
using GoogleServices.Interfaces;
using ServiceLayer.Services.AI;
using static Infrastructure.Helpers.Logger;

namespace ML_SpamClassifier
{
	public class ValidationErrorServiceAI : IValidationErrorServiceAI
	{
		private readonly IGenerativeLanguageModel _generativeLanguageModel;

		public ValidationErrorServiceAI(IGenerativeLanguageModel generativeLanguageModel)
		{
			_generativeLanguageModel = generativeLanguageModel;
		}

		public async Task<string> ExplainInvalidCronExpression(string invalidCroneExpression)
		{
			try
			{
				string prompt = 
				"Ты — эксперт по cron-выражениям и планировщикам задач. Я передаю тебе некорректное cron-выражение в .NET 5, библиотека Quartz. Твоя задача:\n\n"+

				"1. Объясни, \"почему оно некорректное\", с указанием конкретной ошибки (например, неверное количество частей, недопустимые символы и т.д.).\n"+
				"2. Дай \"человеко-понятную расшифровку\", что означает это выражение (если возможно).\n"+
				"3. Предположи, что пользователь хотел запланировать — по названию задачи или по описанию цели (если оно передано).\n"+
				"4. Предложи \"правильный cron-выражение\", которое реализует намерение пользователя.\n"+
				"5. Формат ответа: максимально краткий, понятный, с примерами.\n\n"+

				$"Cron-выражение: \"{invalidCroneExpression}\"";

				// 2. Отправляем запрос к Gemini API
				var response = await _generativeLanguageModel.AskGemini(prompt);

				return response.Trim();
			}
			catch (Exception ex)
			{
				Log("Ошибка при получении тегов от Gemini: " + ex.ToString());
				return null;
			}
		}
	}
}
