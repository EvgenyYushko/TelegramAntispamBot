using System;
using System.Collections.Generic;

namespace DomainLayer.Models
{
	public class TelegramUserEntity
	{
		public long UserId { get; set; }

		public string Name { get; set; }

		public DateTime CreateDate { get; set; }

		public TelegramPermissionsEntity Permissions { get; set; }
	}
}
