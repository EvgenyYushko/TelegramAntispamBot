using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLayer.Models.News;

namespace ServiceLayer.Services.AI
{
	public interface INewsParserServiceAI
	{
		Task<List<string>> GetRelevantTagsAsync(string chatTheme, List<string> availableTags);
		Task<RssFeed> SelectMostRelevantFeedAsync(string chatTitle, string chatDescription, List<string> lastMessages, List<RssFeed> availableFeeds);
	}
}
