using System;
using static Infrastructure.Common.TimeZoneHelper;

namespace Infrastructure.Models
{
	public class ChatMessagesDto
	{
		public Guid Id { get; set; }

		public long MessageId { get; set; }

		public long ChatId { get; set; }

		public long UserId { get; set; }

		public string Text { get; set; } // Текст сообщения

		public DateTime CreatedAt { get; set; } = DateTimeNow; // Дата и время создания записи

		public string ContentType { get; set; }

		public override string ToString()
		{
			return $"MessageId = {MessageId}, Text = {Text}";
		}
	}
}
