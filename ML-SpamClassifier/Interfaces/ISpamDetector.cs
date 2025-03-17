using System.Threading.Tasks;

namespace ML_SpamClassifier.Interfaces
{
	public interface ISpamDetector
	{
		void LoadOrTrainModel();
		Task RetrainModelAsync();
		bool IsSpam(string text, ref string comment);
		public void AddSpamSample(string text);
		public void AddHamSample(string text);
		string GetModelStatus();
	}
}
