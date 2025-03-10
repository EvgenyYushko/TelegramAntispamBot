using System;

namespace DomainLayer.Models
{
	public class TelegramPermissionsEntity
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public long UserId { get; set; }

		public long ChatId {get;set; }

		public bool SendLinks { get; set; }

		public TelegramUserEntity User { get; set; }

		public TelegramChannel Chat { get; set; }
	}
}
