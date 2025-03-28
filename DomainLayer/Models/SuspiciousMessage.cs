using System;
using System.ComponentModel.DataAnnotations;
using static Infrastructure.Common.TimeZoneHelper;

namespace DomainLayer.Models
{
	public class SuspiciousMessage
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
		public string Text { get; set; } // Текст сообщения

		[Required]
		public float Probability { get; set; } // Вероятность спама (от 0 до 1)

		public bool? IsSpamByGemini { get; set; } // Результат проверки Gemini (null, если не проверялось)

		public bool IsSpamByMl { get; set; }

		public bool? IsSpamByUser { get; set; }

		public bool NeedsManualReview { get; set; } = true; // Требуется ли ручная проверка

		[Required]
		public DateTime CreatedAt { get; set; } = DateTimeNow; // Дата и время создания записи
	}
}