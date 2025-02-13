using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Services.Authorization;
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
			// �������� ����� �� ���������� �������
			Console.WriteLine("VkEntryModel-OnGetAsync-Token");
			Token = Request.Query["token"];

			if (!string.IsNullOrEmpty(Token))
			{
				// ���������� ����� ��� ��������� ������ ������������ ����� API VK
				var userInfo = await GetVKUserInfo(Token);
				Console.WriteLine(userInfo);

				// ����� ����� �������� ������ ����������� ������������ �� ����� �����
				// ��������, ��������� ������ � ���� ������
			}
		}

		private async Task<string> GetVKUserInfo(string token)
		{
			using (var httpClient = new HttpClient())
			{
				// ������ � API VK ��� ��������� ���������� � ������������
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
				var claims = result.Principal.Claims;

				// �������� ������ �� VK
				var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
				var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
				var photo = claims.FirstOrDefault(c => c.Type == "urn:vkontakte:photo")?.Value;
				var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

				Console.WriteLine(userId);
				Console.WriteLine(email);
				Console.WriteLine(photo);
				Console.WriteLine(name);

				var user = await _userService.GetUserByName(name);
				if (user is null)
				{
					await _userService.Register(name, email, userId, Role.User.ToString());
				}

				var token = await _userService.Login(email, userId);
				Console.WriteLine($"token={token}");

				HttpContext.Response.Cookies.Append("token", token);

				Console.WriteLine("OnGetCallbackAsync-END");

				return RedirectToPage("/User/Profile");
			}
			return Page();
		}
	}
}
