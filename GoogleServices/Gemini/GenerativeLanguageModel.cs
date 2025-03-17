using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GoogleServices.Gemini.Models;
using GoogleServices.Interfaces;
using Newtonsoft.Json;

namespace GoogleServices.Gemini
{
	public class GenerativeLanguageModel : IGenerativeLanguageModel
	{
		private string _geminiApiKey;
		static readonly HttpClient httpClient = new HttpClient();

		public GenerativeLanguageModel(string geminiApiKey)
		{
			_geminiApiKey = geminiApiKey;
		}

		public async Task<string> AskGemini(string prompt)
		{
			const int maxRetries = 5; // Максимальное количество попыток
			int attempt = 0;

			while (attempt < maxRetries)
			{
				try
				{
					//gemini-2.0-flash
					//gemini-1.5-pro-latest
					var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_geminiApiKey}";

					var request = new
					{
						contents = new[]
						{
					new
					{
						parts = new[]
						{
							new { text = $"{prompt}" }
						}
						}
					},
						generationConfig = new
						{
							temperature = 0.9,
							topK = 40
						}
					};

					var response = await httpClient.PostAsync(
						url,
						new StringContent(
							JsonConvert.SerializeObject(request),
							Encoding.UTF8,
							"application/json"
						)
					);

					// Проверяем статус ответа
					if (!response.IsSuccessStatusCode)
					{
						var errorContent = await response.Content.ReadAsStringAsync();
						if (response.StatusCode == (System.Net.HttpStatusCode)429) // 429 TooManyRequests
						{
							throw new HttpRequestException("Rate limit exceeded", null, response.StatusCode);
						}
						throw new HttpRequestException($"API Error: {response.StatusCode} - {errorContent}");
					}

					var responseContent = await response.Content.ReadAsStringAsync();
					var json = JsonConvert.DeserializeObject<GeminiResponse>(responseContent);

					// Проверяем все уровни вложенности
					if (json?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text is string result)
					{
						return result;
					}

					throw new InvalidDataException("Invalid API response structure");
				}
				catch (HttpRequestException ex) when (ex.StatusCode == (System.Net.HttpStatusCode)429)
				{
					attempt++;
					if (attempt >= maxRetries)
					{
						throw; // Если попытки закончились, выбрасываем исключение
					}

					// Экспоненциальная задержка: 2^attempt секунд
					int delaySeconds = (int)Math.Pow(2, attempt);
					Console.WriteLine($"Rate limit exceeded. Retrying in {delaySeconds} seconds...");
					await Task.Delay(delaySeconds * 1000);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex.Message}");
					throw;
				}
			}

			return null;
		}
	}
}
