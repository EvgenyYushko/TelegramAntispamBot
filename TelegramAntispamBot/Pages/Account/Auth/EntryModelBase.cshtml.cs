using System;
using System.Threading.Tasks;
using Infrastructure.Enumerations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Services.Authorization;

namespace TelegramAntispamBot.Pages.Account.Auth
{
	public class EntryModelBaseModel : PageModel
	{
		private readonly IUserService _userService;

		public EntryModelBaseModel(IUserService userService)
		{
			_userService = userService;
		}

		public async Task<IActionResult> OnGetCallbackAsync()
		{
			var result = await HttpContext.AuthenticateAsync();
			if (result.Succeeded)
			{
				// Получение данных пользователя
				var model = GetRegisterModel(result);

				var user = await _userService.GetUserByName(model.Username);
				if (user is null)
				{
					await _userService.Register(model.Username, model.Email, model.Password, Role.User.ToString());
				}

				var token = await _userService.Login(model.Email, model.Password);

				HttpContext.Response.Cookies.Append("token", token);

				return RedirectToPage("/User/Profile");
			}
			return Page();
		}

		protected virtual EntryModel GetRegisterModel(AuthenticateResult authenticateResult)
		{
			throw new NotImplementedException();
		}
	}

	public struct EntryModel
	{
		public string Username { get; set; }

		public string Password { get; set; }

		public string Email { get; set; }
	}
}
