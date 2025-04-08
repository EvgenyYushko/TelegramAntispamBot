using System;

namespace Infrastructure.Models
{
	public class ChatPermissions
	{
		public Guid Id { get; set; }

		public long ChatId { get; set; }

		public Chanel Chanel { get; set; }

		public bool SendNews { get; set; }

		public bool SendCurrency { get; set; }

		public string CurrencyCronExpression { get; set; }

		public bool SendHabr { get; set; }

		public string HabrCronExpression { get; set; }

		public bool SendOnliner { get; set; }

		public string OnlinerCronExpression { get; set; }
	}
}
