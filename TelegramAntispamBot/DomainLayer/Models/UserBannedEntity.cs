using System;

namespace TelegramAntispamBot.DomainLayer.Models
{
	public class UserBannedEntity
	{
		public long Id { get; set; }

		public string UserName { get; set; }

		public DateTime DateAdd { get; set; }

		public override string ToString() => UserName;
	}
}