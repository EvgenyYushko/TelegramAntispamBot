using System;

namespace TelegramAntispamBot.DomainLayer.Models.Auth
{
	public class UserRoleEntity
	{
		public Guid UserId { get; set; }
		public int RoleId { get; set; }
	}
}