using System.Threading.Tasks;
using DataAccessLayer;
using DomainLayer.Models.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Services.Authorization;
using TelegramAntispamBot.Filters;
using TelegramAntispamBot.Pages.Account.Auth;

namespace TelegramAntispamBot.Pages.Account
{
	[ServiceFilter(typeof(LogPageFilter))]
	public class LoginModel : AuthModelModel
	{
		private readonly IUserService _usersService;

		public LoginModel(IUserService usersService, SignInManager<UserEntity> signInManager, ExternalAuthManager externalAuthManager)
			: base(signInManager, externalAuthManager)
		{
			_usersService = usersService;
		}

		[BindProperty]
		public string UserName { get; set; } = string.Empty;

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

		public async Task<IActionResult> OnPostLogin()
		{
			if (ModelState.IsValid)
			{
				var result = await _usersService.Login(UserName, Password);

				if (result.Succeeded)
				{
					return RedirectToPage("/User/Profile");
				}
				else
				{
					ModelState.AddModelError(string.Empty, "Неверный email или пароль.");
				}
			}
			return Page();
		}
	}
}
