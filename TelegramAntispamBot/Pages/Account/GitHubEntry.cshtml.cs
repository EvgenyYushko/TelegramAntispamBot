using System.Linq;
using System.Security.Claims;
using DataAccessLayer;
using DomainLayer.Models.Authorization;
using Microsoft.AspNetCore.Identity;
using TelegramAntispamBot.Pages.Account.Auth;

namespace TelegramAntispamBot.Pages.Account
{
	public class GitHubEntryModel : AuthModelModel
	{
		public GitHubEntryModel(SignInManager<UserEntity> signInManager, ExternalAuthManager externalAuthManager)
			: base(signInManager, externalAuthManager)
		{
		}

		public void OnGet()
		{
		}

		protected override EntryModel GetRegisterModel(ClaimsPrincipal claimsPrincipal)
		{
			var model = new EntryModel();

			var claims = claimsPrincipal.Claims;

			// Получаем данные из VK
			var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
			var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

			var password = userId + email;

			model.Username = name;
			model.Email = email;

			return model;
		}
	}
}
