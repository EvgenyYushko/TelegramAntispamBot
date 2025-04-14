using System.Collections.Generic;
using Newtonsoft.Json;

namespace GoogleServices.Gemini.Models.Request
{
	public class GeminiRequest
	{
		[JsonProperty("contents")]
		public List<Content> Contents { get; set; } = new List<Content>();

		[JsonProperty("generationConfig")]
		public GenerationConfig GenerationConfig { get; set; } = new GenerationConfig();

		[JsonProperty("safetySettings")]
		public List<SafetySetting> SafetySettings { get; set; } = new List<SafetySetting>();
	}

	public class Content
	{
		[JsonProperty("parts")]
		public List<Part> Parts { get; set; } = new List<Part>();
	}

	public class Part
	{
		[JsonProperty("text")]
		public string Text { get; set; }
	}

	public class GenerationConfig
	{
		[JsonProperty("temperature")]
		public double Temperature { get; set; } = 1.0;

		[JsonProperty("topP")]
		public double TopP { get; set; } = 1.0;

		[JsonProperty("maxOutputTokens")]
		public int MaxOutputTokens { get; set; } = 500;

		[JsonProperty("stopSequences", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> StopSequences { get; set; }
	}

	public class SafetySetting
	{
		[JsonProperty("category")]
		public string Category { get; set; }

		[JsonProperty("threshold")]
		public string Threshold { get; set; }
	}
}
