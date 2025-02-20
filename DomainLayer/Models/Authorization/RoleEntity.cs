using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace DomainLayer.Models.Authorization
{
	public class RoleEntity : IdentityRole<Guid>
	{
		public int IdRole { get; set; }

		public ICollection<PermissionEntity> Permissions { get; set; } = new List<PermissionEntity>();

		public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
	}
}