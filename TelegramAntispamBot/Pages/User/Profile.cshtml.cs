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
		private readonly IUserService _usersService;
		private readonly ITelegramUserService _telegramUserService;

		public ProfileModel(IUserService usersService, ITelegramUserService telegramUserService)
		{
			_usersService = usersService;
			_telegramUserService = telegramUserService;
		}

		[BindProperty]
		public UserAccount CurrentUser { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			if (User.Identity.IsAuthenticated)
			{
				CurrentUser = await _usersService.GetUserById(UserId);
				return Page();
			}

			return RedirectToPage("/Account/Register");
		}
	}
}
