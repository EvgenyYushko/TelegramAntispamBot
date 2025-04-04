namespace ML_SpamClassifier.Models.Gemini.Criteries
{
	internal class SpamCriterion
	{
		public string Id { get; set; } // Уникальный идентификатор (например "C1")
		public string Description { get; set; }
		public double Weight { get; set; } // Важность критерия (0.1 - 1.0)
		public string[] Keywords { get; set; } // Ключевые фразы для быстрой проверки
		public bool CheckLinks { get; set; } // Требуется ли проверка ссылок
	}
}
