using System.Linq;
using System.Security.Claims;
using DataAccessLayer;
using DomainLayer.Models.Authorization;
using Microsoft.AspNetCore.Identity;
using TelegramAntispamBot.Pages.Account.Auth;

namespace TelegramAntispamBot.Pages.Account
{
	public class GoogleEntryModel : AuthModelModel
	{
		public GoogleEntryModel(SignInManager<UserEntity> signInManager, ExternalAuthManager externalAuthManager)
			: base(signInManager, externalAuthManager)
		{
		}

		public void OnGet() { }

		protected override EntryModel GetRegisterModel(ClaimsPrincipal claimsPrincipal)
		{
			var model = new EntryModel();

			var claims = claimsPrincipal.Identities
				.FirstOrDefault()?.Claims.Select(claim => new
				{
					claim.Issuer,
					claim.OriginalIssuer,
					claim.Type,
					claim.Value
				});

			var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
			var gMail = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;

			model.Username = name;
			model.Email = gMail;

			return model;
		}
	}
}
