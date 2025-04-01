using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Enumerations;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServiceLayer.Services.Authorization;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.Pages.Base;
using static Infrastructure.Constants.TelegramConstatns;
using static Infrastructure.Helpers.Logger;

namespace TelegramAntispamBot.Pages.Account
{
	public class TelegramEntryModel : PageModelBase
	{
		private readonly ITelegramUserService _telegramUserService;
		private readonly IUserService _userService;
		private readonly IConfiguration configuration;

		public TelegramEntryModel(IUserService userService, IConfiguration configuration,
			ITelegramUserService telegramUserService)
		{
			_userService = userService;
			this.configuration = configuration;
			_telegramUserService = telegramUserService;
		}

		public async Task<IActionResult> OnGet(
			[FromQuery] long id,
			[FromQuery] string first_name,
			[FromQuery] string last_name,
			[FromQuery] string username,
			[FromQuery] string photo_url,
			[FromQuery] long auth_date,
			[FromQuery] string hash)
		{
			//Log($"id={id}, first_name={first_name}, last_name={last_name}, username={username}, photo_url={photo_url}, auth_date={auth_date}, hash={hash}");
			var botToken = configuration.GetValue<string>(TELEGRAM_ANTISPAM_BOT_KEY) ??
							Environment.GetEnvironmentVariable(TELEGRAM_ANTISPAM_BOT_KEY);
			//Log($"botToken={botToken}");
			//id=1231047171
			//first_name=Evgeny
			//last_name=Yushko
			//username=EvgenyYushko
			//photo_url=https%3A%2F%2Ft.me%2Fi%2Fuserpic%2F320%2FFoI8FHvdVdhA59-KWrYKtuuwIlOAT5BRheEtxz6DeuU.jpg
			//auth_date=1739798735
			//hash=fa54b9e53609322725911c55b7e8e2599b0df6041400982495283a0e7dcc7d1d
			var dataToCheck = new[]
			{
				$"auth_date={auth_date}",
				$"first_name={first_name}",
				$"id={id}",
				$"last_name={last_name}",
				$"photo_url={photo_url}",
				$"username={username}"
			};

			var secretKey = SHA256.HashData(Encoding.UTF8.GetBytes(botToken));
			var hashString = new HMACSHA256(secretKey)
				.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", dataToCheck)))
				.Select(b => b.ToString("x2"))
				.Aggregate((a, b) => a + b);

			//Log($"hashString.Equals(hash, StringComparison.OrdinalIgnoreCase)={hashString.Equals(hash, StringComparison.OrdinalIgnoreCase)}");
			var isValid = hashString.Equals(hash, StringComparison.Ordinal); // Без IgnoreCase
			Log($"Exact match: {isValid}");

			if (hashString.Equals(hash, StringComparison.OrdinalIgnoreCase))
			{
				Log($"UserId = {id} Name = {username} UserSiteId = {UserId}");
				var res = await _telegramUserService.TryAddUserExteranl(new TelegramUser
				{
					UserId = id,
					Name = username,
					UserSiteId = UserId
				});

				if (res)
				{
					Log("user link sucsec");
					var userChats = _telegramUserService.GetChatsByUser(id);
					if (userChats is not null)
					{
						Log("For user finde him chat");

						foreach (var chat in userChats)
						{
							Log(chat);
						}

						if (userChats.Any(c => c.AdminsIds.Contains(id) || c.CreatorId.Equals(id)))
						{
							Log("User was make admin chat!");
							await _userService.UpdateRole(UserId, Role.Tutor);
						}
					}
				}

				Log($"UserId = {id} Name = {username} UserSiteId = {UserId}");

				return RedirectToPage("/User/Profile");
			}

			return RedirectToPage("/User/Profile");
		}
	}
}