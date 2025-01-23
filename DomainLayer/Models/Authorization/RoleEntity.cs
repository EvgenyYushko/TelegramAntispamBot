using System.Collections.Generic;

namespace DomainLayer.Models.Authorization
{
	public class RoleEntity
	{
		public int Id { get; set; }

		public string Name { get; set; } = string.Empty;

		public ICollection<PermissionEntity> Permissions { get; set; } = new List<PermissionEntity>();

		public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
	}
}