using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Infrastructure.Enumerations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TelegramAntispamBot.Pages.Account
{
    public class VkEntryModel : PageModel
    {
        public void OnGet()
        {
        }

		public async Task<IActionResult> OnGetCallbackAsync()
		{
			var result = await HttpContext.AuthenticateAsync();
			if (result.Succeeded)
			{
				var claims = result.Principal.Claims;
        
				// Получаем данные из VK
				var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
				var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
				var photo = claims.FirstOrDefault(c => c.Type == "urn:vkontakte:photo")?.Value;

				//var user = await _userService.GetUserByName(name);
				//if (user is null)
				//{
				//	await _userService.Register(name, gMail, googleId, Role.User.ToString());
				//}

				//var token = await _userService.Login(gMail, googleId);

				//HttpContext.Response.Cookies.Append("token", token);

				return RedirectToPage("/User/Profile");
			}
			return Page();
		}
    }
}
