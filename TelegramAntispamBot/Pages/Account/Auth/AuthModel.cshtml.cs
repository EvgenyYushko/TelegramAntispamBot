using System.Collections.Generic;
using AspNet.Security.OAuth.GitHub;
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
			AuthParams.Add(VkontakteAuthenticationDefaults.AuthenticationScheme, "./VkEntry");
			AuthParams.Add(GitHubAuthenticationDefaults.AuthenticationScheme, "./GitHubEntry");
		}

		public IActionResult OnPostExternalLogin(string provider)
		{
			if(!AuthParams.TryGetValue(provider, out var pageName))
			{
				throw new KeyNotFoundException("AuthenticationScheme not register!");
			}

			var redirectUrl = Url.Page(pageName, pageHandler: "Callback");
			var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
			return Challenge(properties, provider);
		}
	}
}
