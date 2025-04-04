using System.Collections.Generic;
using Newtonsoft.Json;
using static ML_SpamClassifier.SpamDetector;

namespace ML_SpamClassifier.Models.Gemini
{
	internal class AnalysisResult
	{
		[JsonProperty("criteria")]
		public List<CriterionResult> Criteria { get; set; } = new List<CriterionResult>();

		[JsonProperty("modifiers")]
		public List<ModifierResult> Modifiers { get; set; } = new List<ModifierResult>();

		[JsonProperty("total")]
		public double Total { get; set; }

		[JsonProperty("is_spam")]
		public bool IsSpam { get; set; }
	}
}
