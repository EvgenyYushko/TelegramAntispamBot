using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Services;
using ServiceLayer.Services.Authorization;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.Filters;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.Account
{
	[ServiceFilter(typeof(LogPageFilter))]
	public class LoginModel : PageModelBase
	{
		private readonly IUserService _userService;

		public LoginModel(IUserService usersService)
		{
			_userService = usersService;
		}

		[BindProperty]
		public string Email { get; set; } = string.Empty;

		[BindProperty]
		public string Password { get; set; } = string.Empty;

		public async Task<IActionResult> OnGet()
		{
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToPage("/User/Profile");
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			try
			{
				var token = await _userService.Login(Email, Password);

				HttpContext.Response.Cookies.Append("token", token);

				return RedirectToPage("/User/Profile");
			}
			catch (Exception ex)
			{
				ErrorMessage = ex.Message;
				return RedirectToPage("/Account/Login");
			}
		}
	}
}
