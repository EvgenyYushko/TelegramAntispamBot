using System;

namespace DomainLayer.Models.Authorization
{
	public class RolePermissionEntity
	{
		public Guid RoleId { get; set; }
		public int PermissionId { get; set; }
	}
}
