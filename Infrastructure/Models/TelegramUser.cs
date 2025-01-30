using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace Infrastructure.Models
{
	public class TelegramUser
	{
		public long UserId { get; set; }

		public string Name { get; set; }

		public DateTime CreateDate { get; set; }

		public TelegramPermissions Permissions { get; set; }

		public User User { get; set; }

		public PullModel PullModel { get; set; } = new();

		public override bool Equals(object obj)
		{
			return obj is TelegramUser u && u.User.Id == User.Id;
		}
	}
}
