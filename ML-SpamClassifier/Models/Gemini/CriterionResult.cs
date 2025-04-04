using Newtonsoft.Json;

namespace ML_SpamClassifier.Models.Gemini
{
	internal class CriterionResult
	{
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("score")]
		public double Score { get; set; }

		[JsonProperty("reason")]
		public string Reason { get; set; }
	}
}
