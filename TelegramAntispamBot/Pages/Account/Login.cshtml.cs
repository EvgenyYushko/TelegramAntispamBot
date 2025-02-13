using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Vkontakte;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Services.Authorization;
using TelegramAntispamBot.Filters;
using TelegramAntispamBot.Pages.Base;
using static Infrastructure.Helpers.AuthorizeHelper;

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

		public async Task<IActionResult> OnPostLogin()
		{
			try
			{
				var token = await _userService.Login(Email, Password);
				HttpContext.Response.Cookies.Append("token", token);

				var user = await _userService.GetByEmail(Email);
				var claimsIdentity = new ClaimsIdentity(GetClaims(user), CookieAuthenticationDefaults.AuthenticationScheme);
				await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

				return RedirectToPage("/User/Profile");
			}
			catch (Exception ex)
			{
				ErrorMessage = ex.Message;
				return RedirectToPage("/Account/Login");
			}
		}

		public IActionResult OnPostExternalLogin(string provider)
		{
			var redirectUrl = Url.Page("./GoogleEntry", pageHandler: "Callback");
			var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
			return Challenge(properties, GoogleDefaults.AuthenticationScheme);
		}

		public IActionResult OnPostExternalLoginVk(string provider)
		{
			var redirectUrl = Url.Page("./VkEntry", pageHandler: "Callback");
			var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
			return Challenge(properties, VkontakteAuthenticationDefaults.AuthenticationScheme);
		}
	}
}
