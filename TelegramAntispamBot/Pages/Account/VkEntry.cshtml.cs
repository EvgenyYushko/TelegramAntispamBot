using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Infrastructure.Enumerations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Services.Authorization;

namespace TelegramAntispamBot.Pages.Account
{
	public class VkEntryModel : PageModel
	{
		private readonly IUserService _userService;

		public string Token { get; set; }

		public VkEntryModel(IUserService userService)
		{
			this._userService = userService;
		}

		public async Task OnGetAsync()
		{
			// ѕолучаем токен из параметров запроса
			Console.WriteLine("VkEntryModel-OnGetAsync-Token");
			Token = Request.Query["token"];

			if (!string.IsNullOrEmpty(Token))
			{
				// »спользуем токен дл€ получени€ данных пользовател€ через API VK
				var userInfo = await GetVKUserInfo(Token);
				Console.WriteLine(userInfo);

				// «десь можно добавить логику регистрации пользовател€ на вашем сайте
				// Ќапример, сохранить данные в базу данных
			}
		}

		private async Task<string> GetVKUserInfo(string token)
		{
			using (var httpClient = new HttpClient())
			{
				// «апрос к API VK дл€ получени€ информации о пользователе
				var response = await httpClient.GetStringAsync($"https://api.vk.com/method/users.get?access_token={token}&v=5.131");
				return response;
			}
		}

		public void OnGet()
		{
			Console.WriteLine("Console.WriteLine OnGet");
		}

		public async Task<IActionResult> OnGetCallbackAsync()
		{
			Console.WriteLine("OnGetCallbackAsync-START");

			var result = await HttpContext.AuthenticateAsync();
			if (result.Succeeded)
			{
				Console.WriteLine("result.Succeeded=" + result.Succeeded);

				if (result.Principal == null)
				{
					Console.WriteLine("result.Principal is null.");
					return Page();
				}

				var claims = result.Principal.Claims;

				// ѕолучаем данные из VK
				var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
				var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
				var photo = claims.FirstOrDefault(c => c.Type == "urn:vkontakte:photo")?.Value;
				var nameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

				Console.WriteLine("UserId: " + userId);
				Console.WriteLine("Email: " + email);
				Console.WriteLine("Photo: " + photo);
				Console.WriteLine("Name: " + nameClaim);

				if (nameClaim == null)
				{
					Console.WriteLine("ClaimTypes.Name not found in claims.");
				}
				var name = nameClaim.Value;

				if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userId))
				{
					Console.WriteLine("Email or UserId is null or empty.");
				}

				if (_userService == null)
				{
					Console.WriteLine("_userService is null.");
				}

				return Page();


				var user = await _userService.GetUserByName(name);
				if (user is null)
				{
					await _userService.Register(name, email, userId, Role.User.ToString());
				}

				var token = await _userService.Login(email, userId);
				Console.WriteLine($"token={token}");

				if (HttpContext?.Response == null)
				{
					Console.WriteLine("HttpContext or HttpContext.Response is null.");
					return Page();
				}
				HttpContext.Response.Cookies.Append("token", token);

				Console.WriteLine("OnGetCallbackAsync-END");

				return RedirectToPage("/User/Profile");
			}
			return Page();
		}
	}
}
