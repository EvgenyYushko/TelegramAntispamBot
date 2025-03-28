using System;
using System.Threading.Tasks;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Services.Authorization;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.User
{
	[Authorize]
	public class ProfileModel : PageModelBase
	{
		private readonly ITelegramUserService _telegramUserService;
		private readonly IUserService _usersService;

		public ProfileModel(IUserService usersService, ITelegramUserService telegramUserService)
		{
			_usersService = usersService;
			_telegramUserService = telegramUserService;
		}

		[BindProperty]
		public UserAccount CurrentUser { get; set; }

		[BindProperty]
		public TelegramUser TelegramUser { get; set; } = new();

		public async Task<IActionResult> OnGetAsync()
		{
			if (User.Identity.IsAuthenticated)
			{
				Console.WriteLine("ProfileModel-OnGetAsync-UserId=" + UserId);
				CurrentUser = await _usersService.GetUserById(UserId);
				var siteUser = _telegramUserService.GetByUserSiteId(UserId);
				if (siteUser != null)
				{
					Console.WriteLine(siteUser);
					TelegramUser = siteUser;
				}

				//await _telegramUserService.GetChatsByUser(UserId);
				Console.WriteLine(CurrentUser);
				return Page();
			}

			return RedirectToPage("/Account/Register");
		}
	}
}