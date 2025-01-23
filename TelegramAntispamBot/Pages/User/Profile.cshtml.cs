using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Models;
using ServiceLayer.Services.Authorization;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.User
{
	[Authorize]
	public class ProfileModel : PageModelBase
	{
		private readonly IUserService _usersService;

		public ProfileModel(IUserService usersService)
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
