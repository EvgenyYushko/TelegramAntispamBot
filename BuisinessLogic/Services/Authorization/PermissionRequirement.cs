using Infrastructure.Enumerations;
using Microsoft.AspNetCore.Authorization;

namespace BuisinessLogic.Services.Authorization
{
	public class PermissionRequirement : IAuthorizationRequirement
	{
		public PermissionRequirement(Permission permissions)
		{
			Permissions = new[] { permissions };
		}

		public Permission[] Permissions { get; set; }
	}
}