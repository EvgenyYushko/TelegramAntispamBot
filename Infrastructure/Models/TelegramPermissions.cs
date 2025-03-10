using System;

namespace Infrastructure.Models
{
	public class TelegramPermissions
	{
		public Guid Id { get; set; }

		public long UserId { get; set; }

		public long ChatId { get; set; }

		public Chanel Chanel { get; set; } = new();

		public bool SendLinks { get; set; }
	}
}
