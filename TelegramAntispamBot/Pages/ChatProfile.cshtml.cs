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
		private long _chatId;
		private Chanel _chat;
		private ITelegramUserService _telegramUserService;
		public ChatProfileModel(ITelegramUserService telegramUserService)
		{
			_telegramUserService = telegramUserService;
		}
		[BindProperty]
		public Chanel Chat => _chat ??= _telegramUserService.GetChatById(_chatId);

		public async Task<IActionResult> OnGetAsync(long chatId)
		{
			if (User.Identity.IsAuthenticated)
			{
				_chatId = chatId;
				Console.WriteLine(Chat);
				return Page();
			}

			return RedirectToPage("/Account/Register");
		}

		public async Task OnGetSetAllowSendNews(bool allowSendNews, long chatId)
		{
			_chatId = chatId;

			await _telegramUserService.UpdateChatPermissions(new ChatPermissions()
			{
				ChatId = chatId,
				SendNews = allowSendNews
			});
		}
	}
}