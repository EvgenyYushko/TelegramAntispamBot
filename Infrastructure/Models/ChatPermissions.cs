using System;

namespace Infrastructure.Models
{
	public class ChatPermissions
	{
		public Guid Id { get; set; }

		public long ChatId { get; set; }

		public Chanel Chanel { get; set; }

		public bool SendNews { get; set; }
	}
}
