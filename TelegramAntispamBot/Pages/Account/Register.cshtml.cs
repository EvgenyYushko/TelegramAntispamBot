using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Services;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.Account
{
    public class RegisterModel : PageModelBase
    {
		private readonly IUsersService _userService;

		public RegisterModel(IUsersService userService)
		{
			_userService = userService;
		}

		[BindProperty]
		public string Username { get; set; } = string.Empty;

		[BindProperty]
		public string Password { get; set; } = string.Empty;

		[BindProperty]
		public string Email { get; set; } = string.Empty;

		[BindProperty]
		public string SelectedRole { get; set; } = string.Empty;

		public List<string> Roles { get; set; }

		public async Task OnGetAsync()
		{
			Roles = Enum.GetNames(typeof(Role)).Where(role => role != nameof(Role.Admin)).ToList();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			try
			{
				if (ModelState.IsValid)
				{
					await _userService.Register(Username, Email, Password, SelectedRole);
					return RedirectToPage("/Account/Login");
				}
			}
			catch (Exception ex) 
			{
				ErrorMessage = ex.Message;
			}

			return RedirectToPage("/Account/Register");
		}
    }
}
