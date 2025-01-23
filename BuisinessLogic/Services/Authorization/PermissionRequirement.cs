using Infrastructure.Enumerations;
using Microsoft.AspNetCore.Authorization;

namespace BuisinessLogic.Services.Authorization
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
