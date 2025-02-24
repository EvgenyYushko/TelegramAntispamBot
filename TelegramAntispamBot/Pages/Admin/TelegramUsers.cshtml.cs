using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Enumerations;
using Infrastructure.InjectSettings;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.Admin
{
	[Authorize(Policy = nameof(Role.Admin))]
	public class TelegramUsersModel : PageModelBase
	{
		private readonly ITelegramUserService _telegramUserService;
		private TelegramBotClient _telegramClient;

		public TelegramUsersModel(ITelegramUserService telegramUserService, TelegramInject telegramInject)
		{
			_telegramUserService = telegramUserService;
			_telegramClient = telegramInject.TelegramClient;
		}

		public IEnumerable<TelegramUser> TelegramUsers { get; set; } = new List<TelegramUser>();

		public async Task OnGet()
		{
			TelegramUsers = _telegramUserService.GetAllTelegramUsers();
		}

		public async Task OnGetSetRightLiks(long userId, bool sendLinks)
		{
			var user = new TelegramUser
			{
				UserId = userId,
				Permissions = new TelegramPermissions
				{
					UserId = userId,
					SendLinks = sendLinks
				}
			};
			await _telegramUserService.UpdateTelegramUser(user);
			TelegramUsers = _telegramUserService.GetAllTelegramUsers();
			await _telegramUserService.UpdateLocalStorage();
		}

		public async Task OnGetSendMessage(long userId, string message)
		{
			await _telegramClient.SendTextMessageAsync(userId, message);
			return;
		}
	}
}
