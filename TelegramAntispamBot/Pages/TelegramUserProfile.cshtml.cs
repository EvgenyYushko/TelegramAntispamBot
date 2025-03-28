using System.Linq;
using System.Threading.Tasks;
using Infrastructure.InjectSettings;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;

namespace TelegramAntispamBot.Pages
{
	[Authorize]
	public class TelegramUserProfileModel : PageModel
	{
		private TelegramBotClient _telegramClient;
		private ITelegramUserService _telegramUserService;
		private TelegramUser _tgUser;

		private long _userId;

		public TelegramUserProfileModel(ITelegramUserService telegramUserService, TelegramInject telegramInject)
		{
			_telegramUserService = telegramUserService;
			_telegramClient = telegramInject.TelegramClient;
		}

		[BindProperty]
		public long ChatId { get; set; }

		[BindProperty]
		public TelegramUser TgUser
		{
			get
			{
				_tgUser = _tgUser ??= _telegramUserService.Get(_userId);
				return _tgUser;
			}
			set => _tgUser = value;
		}

		public async Task<IActionResult> OnGetAsync(long userId, long chatId)
		{
			if (User.Identity.IsAuthenticated)
			{
				ChatId = chatId;
				_userId = userId;
				return Page();
			}

			return RedirectToPage("/Account/Register");
		}

		public async Task OnGetSetRightLiks(long userId, bool sendLinks, long chatId)
		{
			_userId = userId;
			var chatPer = TgUser.Permissions.FirstOrDefault(p => p.ChatId.Equals(chatId));
			chatPer.SendLinks = sendLinks;

			await _telegramUserService.UpdateTelegramUser(TgUser);
			await _telegramUserService.UpdateLocalStorage();
		}

		public async Task OnGetSendMessage(long userId, string message)
		{
			_userId = userId;
			await _telegramClient.SendTextMessageAsync(userId, message);
		}
	}
}