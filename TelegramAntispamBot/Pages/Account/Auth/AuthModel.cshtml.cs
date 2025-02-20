using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.GitHub;
using AspNet.Security.OAuth.Vkontakte;
using DataAccessLayer;
using DomainLayer.Models.Authorization;
using Infrastructure.Enumerations;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TelegramAntispamBot.Pages.Base;

namespace TelegramAntispamBot.Pages.Account.Auth
{
	public class AuthModelModel : PageModelBase
	{
		private static Dictionary<string, string> AuthParams = new();
		protected readonly SignInManager<UserEntity> _signInManager;
		protected readonly ExternalAuthManager _externalAuthManager;

		public AuthModelModel(SignInManager<UserEntity> signInManager, ExternalAuthManager externalAuthManager)
		{
			_signInManager = signInManager;
			_externalAuthManager = externalAuthManager;
		}

		static AuthModelModel()
		{
			AuthParams.Add(GoogleDefaults.AuthenticationScheme, "/Account/GoogleEntry");
			AuthParams.Add(VkontakteAuthenticationDefaults.AuthenticationScheme, "/Account/VkEntry");
			AuthParams.Add(GitHubAuthenticationDefaults.AuthenticationScheme, "/Account/GitHubEntry");
		}

		public IActionResult OnPostExternalLogin(string provider, string returnUrl)
		{
			if (!AuthParams.TryGetValue(provider, out var pageName))
			{
				throw new KeyNotFoundException("AuthenticationScheme not register!");
			}

			var redirectUrl = Url.Page(pageName, pageHandler: "Callback");
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			properties.Items["ReturnUrl"] = returnUrl;

			return new ChallengeResult(provider, properties);
		}

		public async Task<IActionResult> OnGetCallbackAsync()
		{
			var u = User;
			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				Console.WriteLine("error GetExternalLoginInfoAsync");
				return RedirectToPage("/Account/Login");
			}

			if (!info.AuthenticationProperties.Items.TryGetValue("ReturnUrl", out var redirectUrl))
			{
				Console.WriteLine("redirectUrl is null");
				return RedirectToPage("/Account/Login");
			}

			// Проверяем, есть ли уже существующий связанный аккаунт
			var existingUser = await _externalAuthManager.FindUserByExternalLoginAsync(info.LoginProvider, info.ProviderKey);
			if (existingUser != null)
			{
				// Если аккаунт уже связан - выполняем вход
				await _signInManager.SignInAsync(existingUser, isPersistent: false);
				return LocalRedirect(redirectUrl);
			}

			// Получаем email
			var email = info.Principal.FindFirstValue(ClaimTypes.Email);
			if (string.IsNullOrEmpty(email))
			{
				Console.WriteLine("email is null");
				return RedirectToPage("/Account/Register");
			}

			// Ищем пользователя по email
			var user = await _externalAuthManager.FindUserByEmail(email);
			if (user != null)
			{
				// Проверяем, не связан ли уже этот провайдер с пользователем
				var existingLogin = await _externalAuthManager.ExistsExternalLoginAsync(user.Id, info.LoginProvider);
				if (existingLogin == null)
				{
					await _externalAuthManager.LinkExternalLoginAsync(user, info.LoginProvider, info.ProviderKey);
				}

				// Выполняем вход через SignInManager
				await _signInManager.SignInAsync(user, isPersistent: false);
				return LocalRedirect(redirectUrl);
			}

			// Создаем нового пользователя
			var model = GetRegisterModel(info.Principal);
			var registrationResult = await _externalAuthManager.RegisterExternalUserAsync(model.Username, model.Email, Role.User.ToString());

			if (!registrationResult.Succeeded)
			{
				// Обработка ошибок регистрации
				foreach (var error in registrationResult.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
				return Page();
			}

			var newUser = await _externalAuthManager.FindUserByEmail(email);
			await _externalAuthManager.LinkExternalLoginAsync(newUser, info.LoginProvider, info.ProviderKey);

			// Выполняем вход
			await _signInManager.SignInAsync(newUser, isPersistent: false);
			return LocalRedirect(redirectUrl);
		}

		protected virtual EntryModel GetRegisterModel(ClaimsPrincipal claimsPrincipal)
		{
			throw new NotImplementedException();
		}
	}

	public struct EntryModel
	{
		public string Username { get; set; }

		public string Email { get; set; }

		public override string ToString()
		{
			return $"Username={Username} Email={Email}";
		}
	}
}
