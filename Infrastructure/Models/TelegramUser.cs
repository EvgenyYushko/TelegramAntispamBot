using Telegram.Bot.Types;

namespace Infrastructure.Models
{
	public class TelegramUser
	{
		public User User { get; set; }

		public PullModel PullModel { get; set; } = new();

		public override bool Equals(object obj)
		{
			return obj is TelegramUser u && u.User.Id == User.Id;
		}
	}
}
