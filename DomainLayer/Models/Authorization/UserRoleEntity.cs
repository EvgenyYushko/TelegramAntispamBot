using System;

namespace DomainLayer.Models.Authorization
{
	public class UserRoleEntity
	{
		public Guid UserId { get; set; }
		public int RoleId { get; set; }
	}
}