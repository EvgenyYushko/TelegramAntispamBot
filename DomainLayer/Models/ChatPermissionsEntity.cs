using System;

namespace DomainLayer.Models
{
	public class ChatPermissionsEntity
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public long ChatId { get; set; }

		public TelegramChannel Chat { get; set; }

		public bool SendNews { get; set; }

		public string AllNewsCronExpression { get; set; }

		public bool SendCurrency { get; set; }

		public string CurrencyCronExpression { get; set; }

		public bool SendHabr { get; set; }

		public string HabrCronExpression { get; set; }

		public bool SendOnliner { get; set; }

		public string OnlinerCronExpression { get; set; }
	}
}
