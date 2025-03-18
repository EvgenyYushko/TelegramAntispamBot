using System;

namespace ServiceLayer.Models
{
	public class SuspiciousMessageDto
	{
		public Guid Id { get; set; }

		public string Text { get; set; } // Текст сообщения

		public float Probability { get; set; } // Вероятность спама (от 0 до 1)

		public bool? IsSpamByGemini { get; set; } // Результат проверки Gemini (null, если не проверялось)
		public bool IsSpamByMl { get; set; }

		public bool? IsSpamByUser { get; set; }

		public bool NeedsManualReview { get; set; } // Требуется ли ручная проверка

		public DateTime CreatedAt { get; set; } // Дата и время создания записи
	}
}
