using System.Collections.Generic;
using System.Threading.Tasks;
using DomainLayer.Repositories;
using Infrastructure.Enumerations;
using Microsoft.AspNetCore.Authorization;
using ServiceLayer.Models;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.Admin
{
	[Authorize(Policy = nameof(Role.Admin))]
	public class AnalyticsModel : PageModelBase
	{
		private readonly ILogRepository _logRepository;

		public AnalyticsModel(ILogRepository logRepository)
		{
			_logRepository = logRepository;
		}

		public IEnumerable<Log> Logs { get; set; } = new List<Log>();

		public async Task OnGet()
		{
			Logs = _logRepository.GetLogs();
		}
	}
}