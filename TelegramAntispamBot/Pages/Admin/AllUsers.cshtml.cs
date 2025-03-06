using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Enumerations;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Services.Authorization;

namespace TelegramAntispamBot.Pages.Admin
{
	[Authorize(Policy = nameof(Role.Admin))]
	public class AllUsersModel : PageModel
	{
		private readonly IUserService _userService;

		public AllUsersModel(IUserService userService)
		{
			_userService = userService;
		}

		[BindProperty]
		public List<UserAccount> UsersAccount { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			if (User.Identity.IsAuthenticated)
			{
				UsersAccount = _userService.GetAllUsers();

				return Page();
			}

			return RedirectToPage("/Account/Register");
		}
	}
}
