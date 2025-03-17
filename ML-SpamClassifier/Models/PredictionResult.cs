using Microsoft.ML.Data;

namespace ML_SpamClassifier.Models
{
	public class PredictionResult
	{
		[ColumnName("PredictedLabel")]
		public bool IsSpam;

		public float Probability;
	}
}
