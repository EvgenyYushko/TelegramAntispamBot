using System;

namespace DomainLayer.Models
{
	public class UserChannelMembership
	{
		public long UserId { get; set; }
		public TelegramUserEntity User { get; set; }

		public long ChannelId { get; set; }
		public TelegramChannel Channel { get; set; }

		public DateTime JoinDate { get; set; } = DateTime.UtcNow; // Доп. информация
	}
}
