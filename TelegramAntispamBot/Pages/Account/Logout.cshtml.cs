using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.Account
{
	public class LogoutModel : PageModelBase
	{
		public async Task<IActionResult> OnGetAsync()
		{
			// 1. ������� ��������� ����
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			Response.Cookies.Delete("token");

			// 2. �������� ������������� ������������ Google (���� ����)
			var googleUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var authScheme = User.Identity?.AuthenticationType;

			Console.WriteLine($"LogoutModel-OnGetAsync-authScheme = {authScheme}");

			// 3. ���� ��� Google, ��������� ������
			//if (!string.IsNullOrEmpty(googleUserId) && authScheme == GoogleDefaults.AuthenticationScheme)
			//{
			//	var logoutUrl = $"https://accounts.google.com/Logout?continue=https://appengine.google.com/_ah/logout?continue={Url.Page("/Account/Login", null, null, Request.Scheme)}";
			//	return Redirect(logoutUrl);
			//}

			// 4. ���� ��� �� Google, ������ �������������� �� �������� �����
			return RedirectToPage("/Account/Login");
		}
	}
}
