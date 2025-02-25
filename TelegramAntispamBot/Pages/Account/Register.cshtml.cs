using System;
using System.Threading.Tasks;
using DataAccessLayer;
using DomainLayer.Models.Authorization;
using MailSenderService;
using MailSenderService.ServiceLayer.Services;
using Microsoft.AspNetCore.Http;
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

		[BindProperty]
		public string VerificationCode { get; set; } = string.Empty;

		public async Task OnGetAsync()
		{
		}


		public async Task<IActionResult> OnGetSendVerifyMessage(string email)
		{
			if (!string.IsNullOrWhiteSpace(email))
			{
				var kod = new Random().Next(1, 99);

				HttpContext.Session.SetInt32(nameof(VerificationCode), kod);
				HttpContext.Session.SetString(nameof(Email), email);

				try
				{
					string bodyHtml = HtmlHelper.HTML_TEMPLATE_VERIFY_CODE.Replace("{kod}", kod.ToString());
					await _mailService.Send(email, bodyHtml, "Регистрация в Антиспам бот", true);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					ModelState.AddModelError("", "Ошибка отправки на данный Email");
					return Page();
				}
			}

			return Page();
		}

		public async Task<IActionResult> OnPostRegisterAsync()
		{
			if (ModelState.IsValid)
			{
				var storedCode = HttpContext.Session.GetInt32(nameof(VerificationCode));
				var storedEmail = HttpContext.Session.GetString(nameof(Email));

				if (storedCode?.ToString() != VerificationCode ||
					storedEmail != Email)
				{
					ModelState.AddModelError("", "Неверный код подтверждения");
					return Page();
				}

				// Очищаем сессию после успешной проверки
				HttpContext.Session.Remove(nameof(VerificationCode));
				HttpContext.Session.Remove(nameof(Email));

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
