using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Infrastructure.Common.TimeZoneHelper;

namespace DomainLayer.Models
{
	public class ChatMessagesEntity
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public long MessageId { get; set; }

		public long ChatId { get; set; }

		public TelegramChannel Chat { get; set; }

		public long UserId { get; set; }

		public TelegramUserEntity User { get; set; }

		[Column(TypeName = "text")]
		public string Text { get; set; } // Текст сообщения

		[Required]
		public DateTime CreatedAt { get; set; } = DateTimeNow; // Дата и время создания записи

		public string ContentType { get; set; }

		public override string ToString()
		{
			return $"MessageId = {MessageId}, Text = {Text}";
		}
	}
}
