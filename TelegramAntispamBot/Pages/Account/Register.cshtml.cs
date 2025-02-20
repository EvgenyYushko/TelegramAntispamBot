using System.Threading.Tasks;
using DataAccessLayer;
using DomainLayer.Models.Authorization;
using MailSenderService.ServiceLayer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Services.Authorization;
using TelegramAntispamBot.Pages.Account.Auth;

namespace TelegramAntispamBot.Pages.Account
{
	public class RegisterModel : AuthModelModel
	{
		private readonly IUserService _userService;
		private readonly IMailService _mailService;

		public RegisterModel(IUserService userService, IMailService mailService, SignInManager<UserEntity> signInManager, ExternalAuthManager externalAuthManager)
			: base(signInManager, externalAuthManager)
		{
			_userService = userService;
			_mailService = mailService;
		}

		[BindProperty]
		public string Username { get; set; } = string.Empty;

		[BindProperty]
		public string Password { get; set; } = string.Empty;

		[BindProperty]
		public string Email { get; set; } = string.Empty;

		[BindProperty]
		public string SelectedRole { get; set; } = string.Empty;

		public async Task OnGetAsync()
		{
		}

		//private int _kod;
		//public async Task OnGetSendCheckMail()
		//{
		//	_kod = new Random().Next(1, 99);
		//	try
		//	{
		//		await _mailService.Send(UserName, $"Код регистрации: {_kod}", "Регистрация Антиспам бот");
		//	}
		//	catch (Exception e)
		//	{
		//		Console.WriteLine(e);
		//		throw;
		//	}
		//}

		public async Task<IActionResult> OnPostRegisterAsync()
		{
			if (ModelState.IsValid)
			{

				//if (null)
				//{
				//	"Заполните код регистрации"
				//}

				//if (!_kod.Equals())
				//{
				//	"Не верный код регистрации";
				//}

				var result = await _userService.Register(Username, Email, Password, SelectedRole);
				if (result.Succeeded)
				{
					return RedirectToPage("/Account/Login");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
				return Page();
			}
			return Page();
		}
	}
}
