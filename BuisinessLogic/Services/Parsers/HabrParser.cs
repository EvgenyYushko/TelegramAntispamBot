using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BuisinessLogic.Services.Parsers
{
	public class HabrParser
	{
		/// <summary>
		/// https://habr.com/ru/companies/simbirsoft/articles/
		/// </summary>
		private const string HabrUrl = "https://habr.com/ru/all/";

		public async Task<string> ParseLatestPostAsync()
		{
			using (var httpClient = new HttpClient())
			{
				try
				{
					// Загружаем HTML-страницу
					var html = await httpClient.GetStringAsync(HabrUrl);
					var htmlDoc = new HtmlDocument();
					htmlDoc.LoadHtml(html);

					var allArticles = htmlDoc.DocumentNode.SelectNodes("//article[contains(@class, 'tm-articles-list__item')]");
					var filteredArticles = allArticles?
						.Where(article => article.Descendants("a")
							.Any(a => a.GetAttributeValue("class", "").Contains("tm-publication-hub__link") 
								   && a.Descendants("span")
									   .Any(s => s.InnerText.Contains("Искусственный интеллект") ||
									   s.InnerText.Contains("Машинное обучение") ||
									   s.InnerText.Contains("Программирование") ||
									   s.InnerText.Contains("Тестирование IT-систем")
						)));

					if (filteredArticles.Any())
					{
						// Берем первую статью
						var firstArticle = filteredArticles.First();

						// Извлекаем заголовок и ссылку
						var titleNode = firstArticle.SelectSingleNode(".//h2[contains(@class, 'tm-title')]/a");
						var title = titleNode?.InnerText.Trim();
						var link = "https://habr.com" + titleNode?.GetAttributeValue("href", string.Empty);

						if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(link))
						{
							// Форматируем сообщение
							var message = new StringBuilder();
							message.AppendLine($"📌 *{title}*");
							message.AppendLine($"🔗 [Читать статью]({link})");

							return message.ToString();
						}
					}

					return "❌ Не удалось найти новые посты на Habr.com.";
				}
				catch (Exception ex)
				{
					return $"❌ *Ошибка!* ❌\n`{ex.Message}`";
				}
			}
		}
	}
}