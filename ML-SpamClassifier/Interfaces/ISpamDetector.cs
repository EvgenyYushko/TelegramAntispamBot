using System.Threading.Tasks;

namespace ML_SpamClassifier.Interfaces
{
	public interface ISpamDetector
	{
		Task LoadModel();

		Task TrainModelAsync();

		bool IsSpam(string text, string chatTheme, string chatDescription, ref string comment);
	}
}