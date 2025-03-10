using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Enumerations;
using Infrastructure.InjectSettings;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.Admin
{
	[Authorize(Policy = nameof(Role.Admin))]
	public class TelegramUsersModel : PageModelBase
	{
		private readonly ITelegramUserService _telegramUserService;		

		public TelegramUsersModel(ITelegramUserService telegramUserService)
		{
			_telegramUserService = telegramUserService;
		}

		public IEnumerable<TelegramUser> TelegramUsers { get; set; } = new List<TelegramUser>();

		public async Task OnGet()
		{
			TelegramUsers = _telegramUserService.GetAllTelegramUsers();
		}		
	}
}
