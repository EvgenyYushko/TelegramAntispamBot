using System;
using Microsoft.AspNetCore.Mvc;

namespace TelegramAntispamBot.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class HealthController : ControllerBase
	{
		[HttpGet("/health")]
		public IActionResult GetHealthStatus()
		{
			return Ok($"App is running {DateTime.Now}");
		}
	}
}
