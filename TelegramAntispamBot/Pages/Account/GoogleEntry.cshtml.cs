using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using ServiceLayer.Services.Authorization;
using TelegramAntispamBot.Pages.Account.Auth;

namespace TelegramAntispamBot.Pages.Account
{
	public class GoogleEntryModel : EntryModelBaseModel
	{
		public GoogleEntryModel(IUserService userService)
			: base(userService)
		{
		}

		public void OnGet() { }

		protected override EntryModel GetRegisterModel(AuthenticateResult authenticateResult)
		{
			var model = new EntryModel();

			var claims = authenticateResult.Principal.Identities
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
			model.Password = googleId;

			return model;
		}
	}
}
