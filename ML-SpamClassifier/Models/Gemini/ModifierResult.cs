using Newtonsoft.Json;

namespace ML_SpamClassifier.Models.Gemini
{
	internal class ModifierResult
	{
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("score")]
		public double Score { get; set; }

		[JsonProperty("reason")]
		public string Reason { get; set; }
	}
}
