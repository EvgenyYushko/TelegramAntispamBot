using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace DomainLayer.Models.Authorization
{
	public class UserEntity : IdentityUser<Guid>
	{
		public ICollection<RoleEntity> Roles { get; set; } = new List<RoleEntity>();
		public ICollection<ExternalLoginEntity> ExternalLogins { get; set; } = new List<ExternalLoginEntity>();
	}
}