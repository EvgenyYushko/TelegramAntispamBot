using Telegram.Bot.Types;

namespace TelegramAntispamBot.DomainLayer.Models
{
	public class UserInfo
	{
		public User User { get; set; }

		public PullModel PullModel { get; set; } = new();

		public override bool Equals(object obj)
		{
			return obj is UserInfo u && u.User.Id == User.Id;
		}
	}
}
