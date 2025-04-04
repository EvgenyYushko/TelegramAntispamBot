using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Infrastructure.Models.AI;
using ServiceLayer.Services.AI;

namespace BuisinessLogic.Services.Parsers
{
	public class HabrParser
	{
		/// <summary>
		/// https://habr.com/ru/companies/simbirsoft/articles/
		/// </summary>
		private const string HabrUrl = "https://habr.com/ru/all/";
		private readonly INewsParserServiceAI _newsParserServiceAI;

		public HabrParser(INewsParserServiceAI newsParserServiceAI)
		{
			_newsParserServiceAI = newsParserServiceAI;
		}

		public async Task<string> ParseLatestPostAsync(ParseParams parseParams)
		{
			using (var httpClient = new HttpClient())
			{
				try
				{
					// 1. Загружаем HTML-страницу
					var html = await httpClient.GetStringAsync(HabrUrl);
					var htmlDoc = new HtmlDocument();
					htmlDoc.LoadHtml(html);

					// 2. Собираем все статьи с их тегами
					var allArticles = htmlDoc.DocumentNode.SelectNodes("//article[contains(@class, 'tm-articles-list__item')]");
					if (allArticles == null || !allArticles.Any())
						return "❌ Не удалось найти статьи на Habr.com.";

					// 3. Собираем все уникальные теги
					var allTags = new HashSet<string>();
					var articlesWithTags = new List<ArticleInfo>();

					foreach (var article in allArticles)
					{
						var tagNodes = article.SelectNodes(".//a[contains(@class, 'tm-publication-hub__link')]/span");
						if (tagNodes == null)
							continue;

						var tags = tagNodes.Select(t => t.InnerText.Trim()).Where(t => !t.Equals("*")).ToList();
						tags.ForEach(t => allTags.Add(t));

						articlesWithTags.Add(new ArticleInfo
						{
							Node = article,
							Tags = tags
						});
					}

					// 4. Получаем релевантные теги для чата через Gemini
					List<ArticleInfo> filteredArticles = new();
					var chatTitle = parseParams.ChatTitle;
					var relevantTags = await _newsParserServiceAI.GetRelevantTagsAsync(chatTitle, allTags.ToList());
					if (relevantTags != null && relevantTags.Any())
					{
						// 5. Фильтруем статьи по релевантным тегам
						filteredArticles = articlesWithTags
							.Where(a => a.Tags.Intersect(relevantTags).Any())
							.OrderByDescending(a => a.Node.SelectSingleNode(".//time")?.GetAttributeValue("datetime", ""))
							.ToList();
					}
					else
					{
						// Иначе берём любые
						filteredArticles = articlesWithTags
							.OrderByDescending(a => a.Node.SelectSingleNode(".//time")?.GetAttributeValue("datetime", ""))
							.Take(3)
							.ToList();
					}

					if (!filteredArticles.Any())
						return $"❌ Не найдено статей по тегам: {string.Join(", ", relevantTags)}";

					// 6. Берем самую свежую статью
					var latestArticle = filteredArticles.First();
					var titleNode = latestArticle.Node.SelectSingleNode(".//h2[contains(@class, 'tm-title')]/a");
					var title = titleNode?.InnerText.Trim();
					var link = "https://habr.com" + titleNode?.GetAttributeValue("href", string.Empty);

					if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(link))
						return "❌ Не удалось извлечь данные статьи.";

					// 7. Форматируем сообщение
					var message = new StringBuilder();
					message.AppendLine($"📌 *{title}*");
					//message.AppendLine($"🏷️ Теги: {string.Join(", ", latestArticle.Tags.Intersect(relevantTags))}");
					message.AppendLine($"🔗 [Читать статью]({link})");
					//message.AppendLine($"\nФильтрация по темам: {string.Join(", ", relevantTags)}");

					return message.ToString();
				}
				catch (Exception ex)
				{
					return $"❌ *Ошибка!* ❌\n`{ex.Message}`";
				}
			}
		}

		private class ArticleInfo
		{
			public HtmlNode Node { get; set; }
			public List<string> Tags { get; set; }
		}
	}
}