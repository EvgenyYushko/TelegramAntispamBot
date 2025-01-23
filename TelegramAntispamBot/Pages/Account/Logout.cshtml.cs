using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.Account
{
    public class LogoutModel : PageModelBase
    {
        public async Task<IActionResult> OnGetAsync()
        {
			Response.Cookies.Delete("token");
			return RedirectToPage("/Account/Login");
        }
    }
}
