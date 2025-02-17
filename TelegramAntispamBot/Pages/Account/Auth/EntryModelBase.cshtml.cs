using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Enumerations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
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

		private readonly string _botToken = "���_�����_����"; // ������� � secrets/appsettings

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
            //// 1. ��������� ���-������� �������
            //if (!ValidateTelegramHash(id, first_name, last_name, username, photo_url, auth_date, hash))
            //{
            //    return Unauthorized("Invalid hash");
            //}

            //// 2. ������ ����������� ������������ (��������, ���������� � ������)
            //HttpContext.Session.SetString("TelegramUserId", id.ToString());
            //HttpContext.Session.SetString("TelegramUsername", username);

            //// 3. ��������������� ����� �������� �����������
            return Page();
        }

       

		//public async Task<IActionResult> OnGetCallbackAsync()
		//{
		//	var result = await HttpContext.AuthenticateAsync();
		//	if (result.Succeeded)
		//	{
		//		Console.WriteLine($"result.Succeeded={result.Succeeded}");

		//		// ��������� ������ ������������
		//		var model = GetRegisterModel(result);
		//		Console.WriteLine(model);

		//		var user = await _userService.GetUserByName(model.Username);
		//		if (user is null)
		//		{
		//			await _userService.Register(model.Username, model.Email, model.Password, Role.User.ToString());
		//		}

		//		var token = await _userService.Login(model.Email, model.Password);

		//		Console.WriteLine($"token={token}");

		//		HttpContext.Response.Cookies.Append("token", token);

		//		return RedirectToPage("/User/Profile");
		//	}
		//		Console.WriteLine($"result.Succeeded={result.Succeeded}");

		//	return Page();
		//}

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

		public override string ToString()
		{
			return $"Username={Username} Email={Email} Password={Password}";
		}
	}
}
