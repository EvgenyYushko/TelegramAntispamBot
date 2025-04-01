using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Infrastructure.Helpers.Logger;

namespace TelegramAntispamBot
{
	public static class TelegramAuthExtensions
	{
		public static AuthenticationBuilder AddTelegramAuth(this AuthenticationBuilder builder,
			Action<GitHubAuthenticationOptions> configuration)
		{
			return builder.AddRemoteScheme<TelegramAuthOptions, TelegramAuthHandler>(
				"Telegram",
				"Telegram Auth",
				options => { options.CallbackPath = "/Login"; });
		}
	}

	public class TelegramAuthOptions : RemoteAuthenticationOptions
	{
		public string BotToken { get; set; }
	}

	public class TelegramAuthHandler : RemoteAuthenticationHandler<TelegramAuthOptions>
	{
		public TelegramAuthHandler(IOptionsMonitor<TelegramAuthOptions> options, ILoggerFactory logger,
			UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
		{
		}

		protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
		{
			Log("TelegramAuthHandler-HandleRemoteAuthenticateAsync");
			var user = new UserTeleg
			{
				Id = long.Parse(Request.Form["id"]),
				FirstName = Request.Form["first_name"],
				LastName = Request.Form["last_name"],
				Username = Request.Form["username"],
				Hash = Request.Form["hash"],
				AuthDate = long.Parse(Request.Form["auth_date"])
			};

			var isValid = await ValidateTelegramUserAsync(user);
			if (!isValid)
			{
				return HandleRequestResult.Fail("Invalid Telegram authentication");
			}

			var claims = new List<Claim>
			{
				new(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
				new("Telegram:Username", user.Username)
			};

			var identity = new ClaimsIdentity(claims, Scheme.Name);
			var principal = new ClaimsPrincipal(identity);
			var ticket = new AuthenticationTicket(principal, Scheme.Name);

			return HandleRequestResult.Success(ticket);
		}

		private async Task<bool> ValidateTelegramUserAsync(UserTeleg user)
		{
			var botToken = Options.BotToken;
			var config = Context.RequestServices.GetRequiredService<IConfiguration>();
			var httpClientFactory = Context.RequestServices.GetRequiredService<IHttpClientFactory>();

			// Реализация проверки аналогична предыдущей, с использованием HMAC-SHA256
			// ...
			return true;
		}
	}
}