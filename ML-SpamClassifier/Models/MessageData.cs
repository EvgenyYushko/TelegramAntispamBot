using Microsoft.ML.Data;

namespace ML_SpamClassifier.Models
{
	public class MessageData
	{
		[LoadColumn(0)]
		[ColumnName("Label")]
		public bool IsSpam;

		[LoadColumn(1)]
		public string Text;
	}
}