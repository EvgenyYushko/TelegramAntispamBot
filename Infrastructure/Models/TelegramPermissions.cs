using System;

namespace Infrastructure.Models
{
	public class TelegramPermissions
	{
		public Guid Id { get; set; }

		public long UserId { get; set; }

		public bool SendLinks { get; set; }
	}
}
