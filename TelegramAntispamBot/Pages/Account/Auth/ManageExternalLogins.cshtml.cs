using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer;
using DomainLayer.Models.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace TelegramAntispamBot.Pages.Account.Auth
{
	public class ManageExternalLoginsModel : AuthModelModel
	{
		private readonly SignInManager<UserEntity> _signInManager;
		private readonly UserManager<UserEntity> _userManager;

		public ManageExternalLoginsModel(UserManager<UserEntity> userManager, SignInManager<UserEntity> signInManager,
			ExternalAuthManager externalAuthManager)
			: base(signInManager, externalAuthManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		public IList<ExternalLoginEntity> ExternalLogins { get; set; }

		public IList<AuthenticationScheme> OtherLogins { get; set; }

		public async Task OnGetAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			ExternalLogins = await _externalAuthManager.GetExternalLoginsAsync(user.Id);


			var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
			OtherLogins = schemes.Where(s => ExternalLogins.All(el => el.Provider != s.Name)).ToList();
		}

		public async Task<IActionResult> OnPostRemoveLogin(Guid id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound("User not found.");
			}

			await _externalAuthManager.RemoveExternalLoginAsync(user.Id, id);
			return RedirectToPage();
		}
	}
}