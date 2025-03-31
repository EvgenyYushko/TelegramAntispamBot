using System;

namespace DomainLayer.Models
{
	public class ChatPermissionsEntity
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public long ChatId { get; set; }

		public TelegramChannel Chat { get; set; }

		public bool SendNews { get; set; }
	}
}
