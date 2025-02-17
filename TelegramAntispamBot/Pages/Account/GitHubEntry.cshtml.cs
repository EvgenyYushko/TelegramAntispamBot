using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using ServiceLayer.Services.Authorization;
using TelegramAntispamBot.Pages.Account.Auth;

namespace TelegramAntispamBot.Pages.Account
{
	public class GitHubEntryModel : EntryModelBaseModel
	{
		public GitHubEntryModel(IUserService userService)
			: base(userService)
		{
		}

		public void OnGet()
		{
		}

		protected override EntryModel GetRegisterModel(AuthenticateResult authenticateResult)
		{
			var model = new EntryModel();

			var claims = authenticateResult.Principal.Claims;

			// Получаем данные из VK
			var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
			var name = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").Value;

			var password = userId + email;

			model.Username = name;
			model.Email = email;
			model.Password = password;

			return model;
		} 
	}
}
