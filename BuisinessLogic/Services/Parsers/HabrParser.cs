using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BuisinessLogic.Services.Parsers
{
	public class HabrParser
	{
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

					// Ищем все статьи
					var articles = htmlDoc.DocumentNode.SelectNodes("//article[contains(@class, 'tm-articles-list__item')]");

					if (articles != null && articles.Any())
					{
						// Берем первую статью
						var firstArticle = articles.First();

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
