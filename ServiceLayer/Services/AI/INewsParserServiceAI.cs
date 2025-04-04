using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Services.AI
{
	public interface INewsParserServiceAI
	{
		Task<List<string>> GetRelevantTagsAsync(string chatTheme, List<string> availableTags);
	}
}
