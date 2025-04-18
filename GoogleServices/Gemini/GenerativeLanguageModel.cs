using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GoogleServices.Gemini.Models;
using GoogleServices.Gemini.Models.Request;
using GoogleServices.Interfaces;
using Newtonsoft.Json;
using static Infrastructure.Helpers.Logger;

namespace GoogleServices.Gemini
{
	public class GenerativeLanguageModel : IGenerativeLanguageModel
	{
		private static readonly HttpClient httpClient = new();
		private static readonly Stack<string> modelsStack = new();
		private readonly string _geminiApiKey;
		private readonly bool _isProd;
		private readonly string MEDIA_TYPE = "application/json";

		public GenerativeLanguageModel(string geminiApiKey, bool isProd)
		{
			_geminiApiKey = geminiApiKey;
			_isProd = isProd;
			InitGeminiModels();
			SwitchToNextModel();
		}

		private string GetUrl => $"https://generativelanguage.googleapis.com/v1beta/models/{GeminiModel}:generateContent?key={_geminiApiKey}";
		private string GetUrlDev => "https://telegramantispambot.onrender.com/api/gemini/generate";

		public static string GeminiModel { get; set; }

		private static void SwitchToNextModel()
		{
			GeminiModel = modelsStack.Pop();
			Log($"USE MODEL: {GeminiModel}");
		}

		private static void InitGeminiModels()
		{
			modelsStack.Push("gemma-3-27b-it");
			modelsStack.Push("gemini-1.5-flash-8b");
			modelsStack.Push("learnlm-1.5-pro-experimental");
			modelsStack.Push("gemini-1.5-flash");
			modelsStack.Push("gemini-2.0-flash-exp-image-generation");
			modelsStack.Push("gemini-2.0-flash-thinking-exp-01-21");
			modelsStack.Push("gemini-2.0-flash-lite");
			modelsStack.Push("gemini-2.0-flash");
		}

		public async Task<HttpResponseMessage> GeminiRequest(GeminiRequest request)
		{
			return await Post(JsonConvert.SerializeObject(request));
		}

		public Task<string> GeminiRequest(string prompt)
		{
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
					temperature = 0.1,
					topK = 1,
					topP = 0
					//maxOutputTokens = 10
				}
			};

			return GeminiRequestBase(JsonConvert.SerializeObject(request));
		}

		private async Task<string> GeminiRequestBase(string request)
		{
			const int maxRetries = 5; // Максимальное количество попыток
			var attempt = 0;

			while (attempt < maxRetries)
			{
				try
				{
					var response = await Post(request);

					// Проверяем статус ответа
					if (!response.IsSuccessStatusCode)
					{
						var errorContent = await response.Content.ReadAsStringAsync();
						if (response.StatusCode == (HttpStatusCode)429) // 429 TooManyRequests
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
				catch (HttpRequestException ex) when (ex.StatusCode == (HttpStatusCode)429)
				{
					attempt++;
					if (attempt >= maxRetries)
					{
						if (modelsStack.Count == 0)
						{
							throw; // Если попытки закончились, выбрасываем исключение
						}

						SwitchToNextModel();
						attempt = 0;
						continue;
					}

					// Экспоненциальная задержка: 2^attempt секунд
					var delaySeconds = (int)Math.Pow(2, attempt);
					Log($"Rate limit exceeded. Retrying in {delaySeconds} seconds...");
					await Task.Delay(delaySeconds * 1000);
				}
				catch (Exception ex)
				{
					Log($"Error: {ex.Message}");
					throw;
				}
			}

			return null;
		}

		private Task<HttpResponseMessage> Post(string request)
		{
			var url = _isProd ? GetUrl : GetUrlDev;
			return httpClient.PostAsync(url, new StringContent(request, Encoding.UTF8, MEDIA_TYPE));
		}
	}
}