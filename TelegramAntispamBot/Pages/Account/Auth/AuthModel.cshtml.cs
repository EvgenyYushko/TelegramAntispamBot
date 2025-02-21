using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.GitHub;
using AspNet.Security.OAuth.MailRu;
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
			AuthParams.Add(MailRuAuthenticationDefaults.AuthenticationScheme, "/Account/MailRuEntry");
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

			Console.WriteLine($"redirectUrl={redirectUrl}, returnUrl={returnUrl}");

			return new ChallengeResult(provider, properties);
		}

		public async Task<IActionResult> OnGetCallbackAsync()
		{
			Console.WriteLine("AuthModelModel-OnGetCallbackAsync");

			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				// Логирование ошибки
				return RedirectToPage("/Account/Login");
			}

			Console.WriteLine($"info.LoginProvider={info.LoginProvider}, info.ProviderKey={info.ProviderKey}");

			// Получаем redirectUrl
			if (!info.AuthenticationProperties.Items.TryGetValue("ReturnUrl", out var redirectUrl))
			{
				redirectUrl = Url.Content("~/");
			}

			// Если пользователь уже авторизован, привязываем аккаунт
			if (User.Identity.IsAuthenticated)
			{
				Console.WriteLine($"User.Identity.IsAuthenticated={User.Identity.IsAuthenticated}");

				var currentUser = await _externalAuthManager.FindUserById(UserId);
				if (currentUser == null)
				{
					return NotFound($"Не удалось загрузить пользователя с ID '{UserId}'.");
				}

				Console.WriteLine($"UserId={UserId}");

				// Проверяем, не привязан ли аккаунт к другому пользователю
				var existingLoginUser = await _externalAuthManager.FindUserByExternalLoginAsync(info.LoginProvider, info.ProviderKey);
				if (existingLoginUser != null)
				{
					Console.WriteLine($"existingLoginUser={existingLoginUser.Id}");
					// Ошибка: аккаунт уже связан
					return LocalRedirect(redirectUrl);
				}

				// Привязываем внешний аккаунт
				await _externalAuthManager.LinkExternalLoginAsync(currentUser, info.LoginProvider, info.ProviderKey);

				// Обновляем куки аутентификации
				await _signInManager.RefreshSignInAsync(currentUser);
				return LocalRedirect(redirectUrl);
			}
			else
			{
				// Логика для неавторизованного пользователя (вход/регистрация)
				var existingUser = await _externalAuthManager.FindUserByExternalLoginAsync(info.LoginProvider, info.ProviderKey);
				if (existingUser != null)
				{
					await _signInManager.SignInAsync(existingUser, isPersistent: false);
					return LocalRedirect(redirectUrl);
				}

				var email = info.Principal.FindFirstValue(ClaimTypes.Email);
				if (string.IsNullOrEmpty(email))
					return RedirectToPage("/Account/Register");

				// Поиск пользователя по email
				var user = await _externalAuthManager.FindUserByEmail(email);
				if (user != null)
				{
					// Проверяем, не связан ли уже провайдер
					var existingLogin = await _externalAuthManager.ExistsExternalLoginAsync(user.Id, info.LoginProvider);
					if (existingLogin == null)
						await _externalAuthManager.LinkExternalLoginAsync(user, info.LoginProvider, info.ProviderKey);

					await _signInManager.SignInAsync(user, isPersistent: false);
					return LocalRedirect(redirectUrl);
				}

				// Регистрация нового пользователя
				var model = GetRegisterModel(info.Principal);
				var registrationResult = await _externalAuthManager.RegisterExternalUserAsync(model.Username, model.Email, Role.User.ToString());

				if (!registrationResult.Succeeded)
				{
					foreach (var error in registrationResult.Errors)
						ModelState.AddModelError(string.Empty, error.Description);

					return LocalRedirect(redirectUrl);
				}

				var newUser = await _externalAuthManager.FindUserByEmail(email);
				await _externalAuthManager.LinkExternalLoginAsync(newUser, info.LoginProvider, info.ProviderKey);

				await _signInManager.SignInAsync(newUser, isPersistent: false);
				return LocalRedirect(redirectUrl);
			}
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
