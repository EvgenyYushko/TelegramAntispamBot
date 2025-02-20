using System;
using System.Security.Claims;
using Infrastructure.Constants;
using Infrastructure.Models;

namespace Infrastructure.Helpers
{
	public static class AuthorizeHelper
	{
		public static Claim[] GetClaims(UserAccount user)
		{
			return new Claim[]
			{
				new Claim(CustomClaims.UserId, user.Id.ToString())
			};
		}

		public static string GenerateTemporaryPassword()
		{
			const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
			var random = new Random();
			var chars = new char[12]; // Длина пароля

			for (int i = 0; i < chars.Length; i++)
			{
				chars[i] = validChars[random.Next(0, validChars.Length)];
			}

			return new string(chars);
		}
	}
}
