using Microsoft.AspNetCore.Authorization;
using TelegramAntispamBot.Enumerations;

namespace TelegramAntispamBot.DomainLayer.Models.Auth
{
	public class PermissionRequirement: IAuthorizationRequirement
	{
		public Permission[] Permissions { get; set; }

		public PermissionRequirement(Permission permissions)
		{
			Permissions = new Permission[]{ permissions};
		}
	}
}
