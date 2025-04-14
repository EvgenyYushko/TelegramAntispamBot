using System.Net.Http;
using System.Threading.Tasks;
using GoogleServices.Gemini.Models.Request;

namespace GoogleServices.Interfaces
{
	public interface IGenerativeLanguageModel
	{
		Task<string> GeminiRequest(string prompt);

		Task<HttpResponseMessage> GeminiRequest(GeminiRequest request);
	}
}