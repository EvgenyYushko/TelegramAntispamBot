using System.Collections.Generic;
using AspNet.Security.OAuth.Vkontakte;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.Account.Auth
{
	public class AuthModelModel : PageModelBase
	{
		private static Dictionary<string, string> AuthParams = new();

		static AuthModelModel()
		{
			AuthParams.Add(GoogleDefaults.AuthenticationScheme, "./GoogleEntry");
			AuthParams.Add(VkontakteAuthenticationDefaults.AuthenticationScheme, "/VkEntry");
		}

		public IActionResult OnPostExternalLogin(string provider)
		{
			AuthParams.TryGetValue(provider, out var pageName);

			var redirectUrl = Url.Page(pageName, pageHandler: "Callback");
			var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
			return Challenge(properties, provider);
		}
	}
}
