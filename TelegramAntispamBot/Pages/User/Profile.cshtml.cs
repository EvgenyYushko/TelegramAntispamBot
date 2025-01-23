using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Models;
using ServiceLayer.Services;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.User
{
	[Authorize(Policy = "User")]
    public class ProfileModel : PageModelBase
    {
		private readonly IUsersService _usersService;

		public ProfileModel(IUsersService usersService) 
		{
			_usersService = usersService;
		}

		[BindProperty]
		public UserAccount CurrentUser { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
			if (User.Identity.IsAuthenticated)
			{
				CurrentUser = await _usersService.GetUserById(UserId);

				return Page();
			}

			return RedirectToPage("/Account/Register");
		}
    }
}
