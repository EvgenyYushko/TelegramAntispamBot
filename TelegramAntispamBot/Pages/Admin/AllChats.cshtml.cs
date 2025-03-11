using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Enumerations;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Services.Telegram;

namespace TelegramAntispamBot.Pages.Admin
{
	[Authorize(Policy = nameof(Role.Tutor))]
	public class AllChatsModel : PageModel
	{
		private ITelegramUserService _telegramUserService;

		public AllChatsModel(ITelegramUserService telegramUserService)
		{
			_telegramUserService = telegramUserService;
		}

		[BindProperty]
		public List<Chanel> Chats { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			if (User.Identity.IsAuthenticated)
			{
				Chats = _telegramUserService.GetAllChats();

				return Page();
			}

			return RedirectToPage("/Account/Register");

		}
	}
}
