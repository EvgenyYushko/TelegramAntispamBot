using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServiceLayer.Services.Authorization;
using TelegramAntispamBot.Pages.Account.Auth;
using static Infrastructure.Constants.TelegramConstatns;

namespace TelegramAntispamBot.Pages.Account
{
	public class TelegramEntryModel : EntryModelBaseModel
	{
		private readonly IConfiguration configuration;

		public TelegramEntryModel(IUserService userService, IConfiguration configuration)
			: base(userService)
		{
			this.configuration = configuration;
		}

		public IActionResult OnGet(
		   [FromQuery] long id,
		   [FromQuery] string first_name,
		   [FromQuery] string last_name,
		   [FromQuery] string username,
		   [FromQuery] string photo_url,
		   [FromQuery] long auth_date,
		   [FromQuery] string hash)
		{
			Console.WriteLine($"id={id}, first_name={first_name}, last_name={last_name}, username={username}, photo_url={photo_url}, auth_date={auth_date}, hash={hash}");
			var botToken = configuration.GetValue<string>(TELEGRAM_ANTISPAM_BOT_KEY) ?? Environment.GetEnvironmentVariable(TELEGRAM_ANTISPAM_BOT_KEY);
			Console.WriteLine($"botToken={botToken}");

			var checkHash = auth_date;
			var dataToCheck = new[]
		    {
				$"auth_date={checkHash}",
				$"first_name={first_name}",
				$"id={id}",
				$"last_name={last_name}",
				$"username={username}"
			};

			var secretKey = SHA256.HashData(Encoding.UTF8.GetBytes(botToken));
			var hashString = new HMACSHA256(secretKey)
			   .ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", dataToCheck)))
			   .Select(b => b.ToString("x2"))
			   .Aggregate((a, b) => a + b);

			Console.WriteLine($"hashString.Equals(hash, StringComparison.OrdinalIgnoreCase)={hashString.Equals(hash, StringComparison.OrdinalIgnoreCase)}");
			if (hashString.Equals(hash, StringComparison.OrdinalIgnoreCase))
			{
				return Page();
			}

			return Page();
		}
	}
}
