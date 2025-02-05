using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Infrastructure.Enumerations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Services.Authorization;

namespace TelegramAntispamBot.Pages.Account
{
    public class GoogleEntryModel : PageModel
    {
		private readonly IUserService _userService;

		public GoogleEntryModel(IUserService userService)
		{
			_userService = userService;
		}

        public void OnGet()
        {
        }

		public async Task<IActionResult> OnGetCallbackAsync(bool needRegister)
		{
			var result = await HttpContext.AuthenticateAsync();
			if (result.Succeeded)
			{
				// Получение данных пользователя
				var claims = result.Principal.Identities
					.FirstOrDefault()?.Claims.Select(claim => new
					{
						claim.Issuer,
						claim.OriginalIssuer,
						claim.Type,
						claim.Value
					});

				var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
				var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
				var gMail = claims.FirstOrDefault(c=>c.Type == ClaimTypes.Email).Value;

				if (needRegister)
				{
					var user = await _userService.GetUserByName(name);
					if (user is null)
					{
						await _userService.Register(name, gMail, googleId, Role.User.ToString());
					}
				}

				var token = await _userService.Login(gMail, googleId);

				HttpContext.Response.Cookies.Append("token", token);

				return RedirectToPage("/User/Profile");
			}
			return Page();
		}
    }
}
