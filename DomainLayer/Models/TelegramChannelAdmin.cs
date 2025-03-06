using System.Threading.Channels;
using Telegram.Bot.Types;

namespace DomainLayer.Models
{
	public class TelegramChannelAdmin
	{
		public long UserId { get; set; }
		public TelegramUserEntity User { get; set; }

		public long ChannelId { get; set; }
		public TelegramChannel Channel { get; set; }

		public string Role { get; set; } = "Admin";
	}
}
