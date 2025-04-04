using Newtonsoft.Json;
using static ML_SpamClassifier.SpamDetector;

namespace ML_SpamClassifier.Models.Gemini
{
	internal class GeminiAnalysisResponse
	{
		[JsonProperty("analysis")]
		public AnalysisResult Analysis { get; set; }
	}
}
