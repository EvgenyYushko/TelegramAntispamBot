namespace DomainLayer.Models
{
	public class TelegramChannelAdmin
	{
		public long UserId { get; set; }

		public long ChannelId { get; set; }

		public TelegramUserEntity User { get; set; }

		public TelegramChannel Channel { get; set; }

		public string Role { get; set; } = "Admin";
	}
}