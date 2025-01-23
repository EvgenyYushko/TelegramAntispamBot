using System.Collections.Generic;

namespace TelegramAntispamBot.DomainLayer.Models.Auth
{
	public class PermissionEntity
	{
		public int Id { get; set; }

		public string Name { get; set; } = string.Empty;

		public ICollection<RoleEntity> Roles { get; set; }
	}
}