using Microsoft.AspNetCore.Mvc;
using static Infrastructure.Common.TimeZoneHelper;

namespace TelegramAntispamBot.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class HealthController : ControllerBase
	{
		[HttpGet("/health")]
		public IActionResult GetHealthStatus()
		{
			return Ok($"App is running {DateTimeNow}");
		}
	}
}