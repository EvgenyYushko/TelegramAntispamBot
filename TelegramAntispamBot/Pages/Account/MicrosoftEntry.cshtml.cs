using DataAccessLayer;
using DomainLayer.Models.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TelegramAntispamBot.Pages.Account.Auth;
using System.Linq;

namespace TelegramAntispamBot.Pages.Account
{
    public class MicrosoftEntryModel : AuthModelModel
	{
		public MicrosoftEntryModel(SignInManager<UserEntity> signInManager, ExternalAuthManager externalAuthManager)
			: base(signInManager, externalAuthManager)
		{
		}

		public void OnGet() { }

		protected override EntryModel GetRegisterModel(ClaimsPrincipal claimsPrincipal)
		{
			var model = new EntryModel();

			var claims = claimsPrincipal.Claims;

			// Получаем данные из VK
			var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
			var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;


			model.Username = name;
			model.Email = email;

			return model;
		}
	}
}
