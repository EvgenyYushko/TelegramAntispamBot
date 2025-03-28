using System;
using Microsoft.AspNetCore.Identity;

namespace DomainLayer.Models.Authorization
{
	public class UserRoleEntity : IdentityUserRole<Guid>
	{
		public UserEntity User { get; set; }

		public RoleEntity Role { get; set; }
	}
}