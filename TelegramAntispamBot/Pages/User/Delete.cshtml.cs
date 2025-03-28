using System;
using System.Threading.Tasks;
using DomainLayer.Models.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Services.Authorization;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.User
{
	public class DeleteModel : PageModelBase
	{
		private readonly SignInManager<UserEntity> _signInManager;
		private readonly IUserService _userService;

		public DeleteModel(IUserService userService, SignInManager<UserEntity> signInManager)
		{
			_userService = userService;
			_signInManager = signInManager;
		}

		public async Task<IActionResult> OnGetAsync()
		{
			if (User.Identity.IsAuthenticated)
			{
				Console.WriteLine("ProfileModel-OnGetAsync-UserId=" + UserId);
				await _signInManager.SignOutAsync();
				var res = await _userService.Delete(UserId);

				if (res != null && !res.Succeeded)
				{
					foreach (var err in res.Errors)
					{
						ModelState.AddModelError("", err.Description);
						return Page(); // TODO переделать!
					}
				}
			}

			return RedirectToPage("/Account/Register");
		}
	}
}