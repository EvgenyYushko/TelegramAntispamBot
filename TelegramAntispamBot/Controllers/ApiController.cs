using System;
using System.Net.Http;
using System.Threading.Tasks;
using GoogleServices.Gemini.Models.Request;
using GoogleServices.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TelegramAntispamBot.Controllers
{
	[ApiController]
	[Route("api/gemini")]
	public class ApiController : Controller
	{
		private readonly IGenerativeLanguageModel _generativeLanguageModel;

		public ApiController(IGenerativeLanguageModel generativeLanguageModel)
		{
			_generativeLanguageModel = generativeLanguageModel;
		}

		[HttpPost("generate")]
		public async Task<IActionResult> GenerateContent([FromBody] GeminiRequest requestModel)
		{
			try
			{
				HttpResponseMessage response;
				try
				{
					response = await _generativeLanguageModel.GeminiRequest(requestModel);
				}
				catch (HttpRequestException ex)
				{
					return StatusCode(503, new
					{
						success = false,
						error = $"Сетевой сбой при попытке соединения с Gemini API: {ex.Message}",
						tip = "Возможно, ваш регион ограничивает доступ. Попробуйте использовать VPN или другой IP."
					});
				}
				catch (TaskCanceledException ex)
				{
					return StatusCode(504, new
					{
						success = false,
						error = $"Превышено время ожидания ответа от Gemini API: {ex.Message}",
						tip = "Проверьте интернет-соединение или увеличьте таймаут в конфигурации HttpClient."
					});
				}

				if (!response.IsSuccessStatusCode)
				{
					var statusCode = (int)response.StatusCode;
					var errorContent = await response.Content.ReadAsStringAsync();

					return StatusCode(statusCode, new
					{
						success = false,
						error = $"Gemini API вернул ошибку: {errorContent}",
						statusCode,
						tip = statusCode == 403
							? "Возможен региональный бан или неправильный API-ключ."
							: statusCode == 429
								? "Превышен лимит запросов. Подождите и повторите позже."
								: null
					});
				}

				var responseContent = await response.Content.ReadAsStringAsync();

				JObject jsonResponse;
				try
				{
					jsonResponse = JObject.Parse(responseContent);
				}
				catch (JsonReaderException ex)
				{
					return StatusCode(500, new
					{
						success = false,
						error = $"Ошибка при разборе ответа от Gemini API: {ex.Message}",
						raw = responseContent
					});
				}

				return Ok(new
				{
					success = true,
					text = jsonResponse
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					success = false,
					error = $"Непредвиденная ошибка: {ex.Message}"
				});
			}
		}
	}
}

