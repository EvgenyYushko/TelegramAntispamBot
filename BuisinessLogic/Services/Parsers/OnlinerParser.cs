using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BuisinessLogic.Services.Parsers
{
	public class OnlinerParser
	{
		private const string OnlinerUrl = "https://realt.onliner.by";

		public async Task<string> ParseLatestPostAsync()
		{
			using (var httpClient = new HttpClient())
			{
				try
				{
					// Загружаем HTML-страницу
					var html = await httpClient.GetStringAsync(OnlinerUrl);
					var htmlDoc = new HtmlDocument();
					htmlDoc.LoadHtml(html);

					// Ищем все новости
					var newsItems = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'news-tidings__subtitle')]");

					if (newsItems != null && newsItems.Any())
					{
						// Берем первую новость
						var firstNews = newsItems.First();

						// Извлекаем заголовок и ссылку
						var titleNode = firstNews.SelectSingleNode(".//a[contains(@class, 'news-tidings__link')]");
						var title = titleNode?.InnerText.Trim();
						var link = OnlinerUrl + titleNode?.GetAttributeValue("href", string.Empty);

						if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(link))
						{
							// Форматируем сообщение
							var message = new StringBuilder();
							message.AppendLine($"📌 *{title}*");
							message.AppendLine($"🔗 [Читать новость]({link})");

							return message.ToString();
						}
					}

					return "❌ Не удалось найти новые новости на Onliner.by.";
				}
				catch (Exception ex)
				{
					return $"❌ *Ошибка!* ❌\n`{ex.Message}`";
				}
			}
		}
	}
}