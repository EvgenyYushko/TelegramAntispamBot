using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ML_SpamClassifier.Helpers
{
	internal class TextRepetitionAnalyzer
	{
		private static readonly Regex _wordRegex = new Regex(@"\w+", RegexOptions.Compiled);
		private static readonly HashSet<string> _excludedWords = new HashSet<string>
		{
			"и", "в", "на", "с", "не", "а", "но", "или"
		};

		public double CalculateRepetitionScore(string message)
		{
			if (string.IsNullOrWhiteSpace(message))
				return 0;

			var words = _wordRegex.Matches(message.ToLower())
				.Select(m => m.Value)
				.Where(w => !_excludedWords.Contains(w))
				.ToArray();

			if (words.Length == 0)
				return 0;

			// 1. Анализ повторений отдельных слов
			var wordFrequency = words
				.GroupBy(w => w)
				.ToDictionary(g => g.Key, g => g.Count());

			double repetitionScore = 0;

			// 2. Прогрессивный штраф за повторяющиеся слова
			foreach (var pair in wordFrequency)
			{
				if (pair.Value >= 10) // Очень частые повторения
					repetitionScore += 0.5;
				else if (pair.Value >= 7)
					repetitionScore += 0.3;
				else if (pair.Value >= 5)
					repetitionScore += 0.2;
				else if (pair.Value >= 3)
					repetitionScore += 0.1;
			}

			// 3. Штраф за общую однообразность (коэффициент уникальности)
			double uniquenessRatio = (double)wordFrequency.Count / words.Length;

			if (uniquenessRatio < 0.2) // Менее 20% уникальных слов
				repetitionScore += 0.4;
			else if (uniquenessRatio < 0.4)
				repetitionScore += 0.2;
			else if (uniquenessRatio < 0.6)
				repetitionScore += 0.1;

			// 4. Проверка на "залипание" клавиш (ааааа, пжжж)
			var charRepeatScore = CalculateCharacterRepetitionScore(message);
			repetitionScore += charRepeatScore;

			// Ограничиваем максимальный штраф
			return Math.Min(repetitionScore, 1.0);
		}

		private double CalculateCharacterRepetitionScore(string message)
		{
			double score = 0;
			var charGroups = Regex.Matches(message, @"(\w)\1{2,}"); // Ищем 3+ повторений подряд

			foreach (Match match in charGroups)
			{
				int repeatCount = match.Length;

				if (repeatCount >= 10)
					score += 0.3;
				else if (repeatCount >= 7)
					score += 0.2;
				else if (repeatCount >= 5)
					score += 0.1;
				else if (repeatCount >= 3)
					score += 0.05;
			}

			return Math.Min(score, 0.3);
		}
	}
}
