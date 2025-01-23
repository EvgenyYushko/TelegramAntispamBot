using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.User
{
    public class GuestModel : PageModelBase
    {
        public async Task<IActionResult> OnGet()
        {
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToPage("/User/Profile");
			}

			return Page();
        }
    }
}
