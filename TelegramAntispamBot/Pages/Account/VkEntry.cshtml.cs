using System;
using System.Linq;
using System.Security.Claims;
using DataAccessLayer;
using DomainLayer.Models.Authorization;
using Microsoft.AspNetCore.Identity;
using TelegramAntispamBot.Pages.Account.Auth;

namespace TelegramAntispamBot.Pages.Account
{
	public class VkEntryModel : AuthModelModel
	{
		public VkEntryModel(SignInManager<UserEntity> signInManager, ExternalAuthManager externalAuthManager)
			: base(signInManager, externalAuthManager)
		{
		}

		public void OnGet() { }

		protected override EntryModel GetRegisterModel(ClaimsPrincipal claimsPrincipal)
		{
			var model = new EntryModel();

			var claims = claimsPrincipal.Claims;

			// Получаем данные из VK
			var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
			var photo = claims.FirstOrDefault(c => c.Type == "urn:vkontakte:photo")?.Value;
			var name = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname").Value;
			var surname = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname").Value;
			var test = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

			foreach (var item in claimsPrincipal.Claims)
			{
				Console.WriteLine($"{item.Type}={item.Value}");
			}

			var userName = $"{name} {surname}";

			model.Username = userName;
			model.Email = email;

			return model;
		}
	}
}
