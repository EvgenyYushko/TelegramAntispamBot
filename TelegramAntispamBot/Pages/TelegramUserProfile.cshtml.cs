using System.Threading.Tasks;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Services.Telegram;
using Telegram.Bot.Types;

namespace TelegramAntispamBot.Pages
{
	[Authorize]
    public class TelegramUserProfileModel : PageModel
    {
		private ITelegramUserService _telegramUserService;

		public TelegramUserProfileModel(ITelegramUserService telegramUserService)
		{
			_telegramUserService = telegramUserService;
		}

		[BindProperty]
		public TelegramUser TelegramUser { get; set; }	

		public async Task<IActionResult> OnGetAsync(long userId, long chatId)
		{
			if (User.Identity.IsAuthenticated)
			{
				TelegramUser = _telegramUserService.Get(userId);
				return Page();
			}

			return RedirectToPage("/Account/Register");
		}
    }
}
