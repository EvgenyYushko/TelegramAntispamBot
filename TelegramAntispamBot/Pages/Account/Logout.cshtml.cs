using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.Account
{
	public class LogoutModel : PageModelBase
	{
		public async Task<IActionResult> OnGetAsync()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			Response.Cookies.Delete("token");
			//await HttpContext.SignOutAsync(GoogleDefaults.AuthenticationScheme);
			return RedirectToPage("/Account/Login");
		}
	}
}
