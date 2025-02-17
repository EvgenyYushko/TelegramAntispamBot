using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Services.Authorization;
using TelegramAntispamBot.Pages.Account.Auth;

namespace TelegramAntispamBot.Pages.Account
{
    public class TelegramEntryModel : EntryModelBaseModel
	{
		public TelegramEntryModel(IUserService userService)
			: base(userService)
		{
		}

		 public IActionResult OnGet(
            [FromQuery] long id,
            [FromQuery] string first_name,
            [FromQuery] string last_name,
            [FromQuery] string username,
            [FromQuery] string photo_url,
            [FromQuery] long auth_date,
            [FromQuery] string hash)
        {
			Console.WriteLine($"id={id}, first_name={first_name}, last_name={last_name}, username={username}, photo_url={photo_url}, auth_date={auth_date}, hash={hash}");
            //// 1. Проверяем хеш-подпись запроса
            //if (!ValidateTelegramHash(id, first_name, last_name, username, photo_url, auth_date, hash))
            //{
            //    return Unauthorized("Invalid hash");
            //}

            //// 2. Логика авторизации пользователя (например, сохранение в сессию)
            //HttpContext.Session.SetString("TelegramUserId", id.ToString());
            //HttpContext.Session.SetString("TelegramUsername", username);

            //// 3. Перенаправление после успешной авторизации
            return Page();
        }

		//protected override EntryModel GetRegisterModel(AuthenticateResult authenticateResult)
		//{
		//	var model = new EntryModel();

		//	var claims = authenticateResult.Principal.Claims;

		//	// Получаем данные из VK
		//	var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
		//	var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
		//	var photo = claims.FirstOrDefault(c => c.Type == "urn:vkontakte:photo")?.Value;
		//	var name = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname").Value;
		//	var surname = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname").Value;

		//	var userName = $"{name} {surname}";
		//	var password = userId + email;

		//	model.Username = userName;
		//	model.Email = email;
		//	model.Password = password;

		//	return model;
		//}
	}
}
