using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Infrastructure.Enumerations;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Services.Telegram;
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

		public async Task OnGetSetRightLiks(long userId, bool sendLinks)
		{
			var user = new TelegramUser
			{
				UserId = userId,
				Permissions = new TelegramPermissions
				{
					UserId = userId,
					SendLinks = sendLinks
				}
			};
			await _telegramUserService.UpdateTelegramUser(user);
			TelegramUsers = _telegramUserService.GetAllTelegramUsers();
			await _telegramUserService.UpdateLocalStorage();
		}
    }
}
