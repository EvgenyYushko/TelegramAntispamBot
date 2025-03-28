using System.Threading.Tasks;

namespace GoogleServices.Interfaces
{
	public interface IGenerativeLanguageModel
	{
		Task<string> AskGemini(string prompt);
	}
}