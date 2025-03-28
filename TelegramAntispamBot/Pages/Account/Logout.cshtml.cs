using System;
using System.Threading.Tasks;
using DomainLayer.Models.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.Account
{
	public class LogoutModel : PageModelBase
	{
		private readonly SignInManager<UserEntity> _signInManager;

		public LogoutModel(SignInManager<UserEntity> signInManager)
		{
			_signInManager = signInManager;
		}

		public async Task<IActionResult> OnGetAsync()
		{
			var authScheme = User.Identity?.AuthenticationType;

			Console.WriteLine($"LogoutModel-OnGetAsync-authScheme = {authScheme}");

			// 1. Выход из локальной аутентификации (куки)
			await _signInManager.SignOutAsync();
			Response.Cookies.Delete("token");

			// 2. Получаем идентификатор пользователя Google (если есть)
			//var googleUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


			// Если пользователь вошел через внешнего провайдера, завершаем сессию
			//if (!string.IsNullOrEmpty(authScheme) && authScheme != CookieAuthenticationDefaults.AuthenticationScheme)
			//{
			//	await HttpContext.SignOutAsync(authScheme);
			//}

			// 3. Если это Google, завершаем сессию
			//if (!string.IsNullOrEmpty(googleUserId) && authScheme == GoogleDefaults.AuthenticationScheme)
			//{
			//	var logoutUrl = $"https://accounts.google.com/Logout?continue=https://appengine.google.com/_ah/logout?continue={Url.Page("/Account/Login", null, null, Request.Scheme)}";
			//	return Redirect(logoutUrl);
			//}

			// 4. Если это не Google, просто перенаправляем на страницу входа
			return RedirectToPage("/Account/Login");
		}
	}
}