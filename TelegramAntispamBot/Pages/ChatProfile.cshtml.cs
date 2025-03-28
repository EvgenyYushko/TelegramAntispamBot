using System;
using System.Threading.Tasks;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Services.Telegram;

namespace TelegramAntispamBot.Pages
{
	[Authorize]
	public class ChatProfileModel : PageModel
	{
		private ITelegramUserService _telegramUserService;

		public ChatProfileModel(ITelegramUserService telegramUserService)
		{
			_telegramUserService = telegramUserService;
		}

		[BindProperty]
		public Chanel Chat { get; set; }

		public async Task<IActionResult> OnGetAsync(long chatId)
		{
			if (User.Identity.IsAuthenticated)
			{
				Chat = _telegramUserService.GetChatById(chatId);
				Console.WriteLine(Chat);
				return Page();
			}

			return RedirectToPage("/Account/Register");
		}
	}
}