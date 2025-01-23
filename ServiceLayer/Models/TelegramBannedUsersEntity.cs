using System;

namespace ServiceLayer.Models
{
	public class TelegramBannedUsersEntity
	{
		public long Id { get; set; }

		public string UserName { get; set; }

		public DateTime DateAdd { get; set; }

		public override string ToString() => UserName;
	}
}