using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TelegramAntispamBot.Pages.Account
{
	public class VkEntryModel : PageModel
	{
		public string Token { get; set; }

		public async Task OnGetAsync()
		{
			// �������� ����� �� ���������� �������
			Token = Request.Query["token"];
			Console.WriteLine("Token");

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
			var result = await HttpContext.AuthenticateAsync();
			if (result.Succeeded)
			{
				Console.WriteLine("result.Succeeded=" + result.Succeeded);
				var claims = result.Principal.Claims;

				// �������� ������ �� VK
				var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
				var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
				var photo = claims.FirstOrDefault(c => c.Type == "urn:vkontakte:photo")?.Value;
				Console.WriteLine(email);
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
