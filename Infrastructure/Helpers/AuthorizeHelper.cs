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
	}
}
