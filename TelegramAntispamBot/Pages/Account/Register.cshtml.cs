using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Enumerations;
using MailSenderService.ServiceLayer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Services.Authorization;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.Account
{
	public class RegisterModel : PageModelBase
	{
		private readonly IUserService _userService;
		private readonly IMailService _mailService;

		public RegisterModel(IUserService userService, IMailService mailService)
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

		public List<string> Roles { get; set; }

		public async Task OnGetAsync()
		{
			Roles = Enum.GetNames(typeof(Role)).Where(role => role != nameof(Role.Admin)).ToList();
		}

		//private int _kod;
		//public async Task OnGetSendCheckMail()
		//{
		//	_kod = new Random().Next(1, 99);
		//	try
		//	{
		//		await _mailService.Send(Email, $"Код регистрации: {_kod}", "Регистрация Антиспам бот");
		//	}
		//	catch (Exception e)
		//	{
		//		Console.WriteLine(e);
		//		throw;
		//	}
		//}

		public async Task<IActionResult> OnPostRegisterAsync()
		{
			try
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

		public IActionResult OnPostExternalLogin(string provider)
		{
			var redirectUrl = Url.Page("./GoogleEntry", pageHandler: "Callback", values: new {needRegister = true});
			var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
			return Challenge(properties, provider);
		}
	}
}
