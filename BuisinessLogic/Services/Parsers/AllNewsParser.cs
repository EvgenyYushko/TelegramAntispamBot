using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Infrastructure.Models.AI;
using ServiceLayer.Models.News;
using ServiceLayer.Services.AI;

namespace BuisinessLogic.Services.Parsers
{
	public class AllNewsParser
	{
		private readonly INewsParserServiceAI _newsParserServiceAI;

		public AllNewsParser(INewsParserServiceAI newsParserServiceAI)
		{
			_newsParserServiceAI = newsParserServiceAI;
		}

		public async Task<string> ParseAllNewsRss(ParseParams parseParams)
		{
			var allFeeds = RssFeeds.GetAllFeeds();

			var rssfeed = await _newsParserServiceAI.SelectMostRelevantFeedAsync(parseParams.ChatTitle, parseParams.ChatDescription, parseParams.lastMessages, allFeeds);

			SyndicationFeed feed;
			using (var reader = XmlReader.Create(rssfeed.Url))
			{
				feed = SyndicationFeed.Load(reader);
				Console.WriteLine($"Канал: {feed.Title.Text}");

				//item = feed.Items.OrderBy(f => f.PublishDate).FirstOrDefault();

				//Console.WriteLine($"\nЗаголовок: {item.Title.Text}");
				//Console.WriteLine($"Ссылка: {item.Links[0].Uri}");
				//Console.WriteLine($"Дата: {item.PublishDate.DateTime}");
				//Console.WriteLine($"Кратко: {item.Summary?.Text}");
			}

			var item = await _newsParserServiceAI.GetMostRelevantNewsItemAsync(feed, parseParams.ChatTitle, parseParams.ChatDescription, parseParams.lastMessages);

			var title = item.Title.Text;
			var link = item.Links[0].Uri.ToString();
			string fixedUrl = CleanUrl(link); 

			if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(fixedUrl))
				return "❌ Не удалось извлечь данные статьи.";

			// 7. Форматируем сообщение
			var message = new System.Text.StringBuilder();
			message.AppendLine($"📌 *{title}*");
			//message.AppendLine($"🏷️ Теги: {string.Join(", ", latestArticle.Tags.Intersect(relevantTags))}");
			message.AppendLine($"🔗 [Читать статью]({fixedUrl})");
			//message.AppendLine($"\nФильтрация по темам: {string.Join(", ", relevantTags)}");

			return message.ToString();

			//foreach (var feed in allFeeds)
			//{
			//	Console.WriteLine($"{feed.Name} ({feed.Category}): {feed.Url}");
			//}

			//var feedsByCategory = RssFeeds.GetFeedsByCategory();
			//foreach (var category in feedsByCategory)
			//{
			//	Console.WriteLine($"\nКатегория: {category.Key}");
			//	foreach (var feed in category.Value)
			//	{
			//		Console.WriteLine($"- {feed.Name}: {feed.Description}");
			//	}
			//}

			//var nonVpnFeeds = RssFeeds.GetAllFeeds().Where(f => !f.RequiresVpn).ToList();

			//var techFeeds = RssFeeds.GetAllFeeds()
			//	.Where(f => f.Category == "Технологии")
			//	.ToList();

			//string rssUrl = "https://habr.com/ru/rss/hub/csharp/";

			//using (var reader = XmlReader.Create(rssUrl))
			//{
			//	var feed = SyndicationFeed.Load(reader);
			//	Console.WriteLine($"Канал: {feed.Title.Text}");

			//	foreach (var item in feed.Items)
			//	{
			//		Console.WriteLine($"\nЗаголовок: {item.Title.Text}");
			//		Console.WriteLine($"Ссылка: {item.Links[0].Uri}");
			//		Console.WriteLine($"Дата: {item.PublishDate.DateTime}");
			//		Console.WriteLine($"Кратко: {item.Summary?.Text}");
			//	}
			//}

			return null;
		}

		public static string CleanUrl(string url)
		{
			var sb = new StringBuilder();
			foreach (char c in url)
			{
				if (!char.IsControl(c) && c != '\u200E') // Удаляем управляющие символы и U+200E
					sb.Append(c);
			}
			return sb.ToString();
		}
	}

	public static class RssFeeds
	{
		public static List<RssFeed> GetAllFeeds()
		{
			return new List<RssFeed>
		{
            // 1. Новости и СМИ
			new RssFeed
			{
				Name = "Onliner auto",
				Url = "https://rss.app/feeds/pIBSlFgadQRwYyuG.xml",
				Description = "Медиакомпания в Минске с новостями об автомобилях",
				Category = "Новости",
				RequiresVpn = false
			},
			new RssFeed
			{
				Name = "Onliner",
				Url = "https://rss.app/feeds/AACxt9fRfZNPyXyg.xml",
				Description = "Медиакомпания в Минске",
				Category = "Новости",
				RequiresVpn = false
			},
			new RssFeed
			{
				Name = "РИА Новости",
				Url = "https://ria.ru/export/rss2/index.xml",
				Description = "Главные новости России и мира",
				Category = "Новости",
				RequiresVpn = false
			},
			new RssFeed
			{
				Name = "ТАСС",
				Url = "https://tass.ru/rss/v2.xml",
				Description = "Официальные новости",
				Category = "Новости",
				RequiresVpn = false
			},
			new RssFeed
			{
				Name = "Коммерсантъ",
				Url = "https://www.kommersant.ru/RSS/news.xml",
				Description = "Деловые и политические новости",
				Category = "Новости",
				RequiresVpn = false
			},
			new RssFeed
			{
				Name = "Медуза",
				Url = "https://meduza.io/rss/all",
				Description = "Независимые новости",
				Category = "Новости",
				RequiresVpn = true
			},
			new RssFeed
			{
				Name = "Lenta.ru",
				Url = "https://lenta.ru/rss",
				Description = "Последние события в России и мире",
				Category = "Новости",
				RequiresVpn = false
			},

            // 2. Технологии и IT
            new RssFeed
			{
				Name = "Хабрахабр (все статьи)",
				Url = "https://habr.com/ru/rss/all/all/",
				Description = "Все статьи Хабра",
				Category = "Технологии",
				RequiresVpn = false
			},
			new RssFeed
			{
				Name = "Хабрахабр (C#)",
				Url = "https://habr.com/ru/rss/hub/csharp/",
				Description = "Статьи по C#",
				Category = "Технологии",
				RequiresVpn = false
			},
			new RssFeed
			{
				Name = "3DNews",
				Url = "https://3dnews.ru/breaking/rss/",
				Description = "Новости hardware и софта",
				Category = "Технологии",
				RequiresVpn = false
			},
			new RssFeed
			{
				Name = "VC.ru",
				Url = "https://vc.ru/rss",
				Description = "Стартапы, бизнес в IT",
				Category = "Технологии",
				RequiresVpn = false
			},
			new RssFeed
			{
				Name = "OpenNET",
				Url = "https://www.opennet.ru/opennews/opennews_full.rss",
				Description = "Новости open source",
				Category = "Технологии",
				RequiresVpn = false
			},
			//new RssFeed
			//{
			//	Name = "DTF (игры)",
			//	Url = "https://dtf.ru/rss/games",
			//	Description = "Игровые новости",
			//	Category = "Технологии",
			//	RequiresVpn = false
			//},

            // 3. Наука и образование
            new RssFeed
			{
				Name = "N+1",
				Url = "https://nplus1.ru/rss",
				Description = "Научные статьи и исследования",
				Category = "Наука",
				RequiresVpn = false
			},
			new RssFeed
			{
				Name = "ПостНаука",
				Url = "https://postnauka.ru/feed",
				Description = "Популярная наука",
				Category = "Наука",
				RequiresVpn = false
			},
			new RssFeed
			{
				Name = "Элементы",
				Url = "https://elementy.ru/rss/news",
				Description = "Физика, биология, математика",
				Category = "Наука",
				RequiresVpn = false
			},

            // 4. Развлечения и культура
   //         new RssFeed
			//{
			//	Name = "Кинопоиск",
			//	Url = "https://www.kinopoisk.ru/rss/feed/news/",
			//	Description = "Новости кино",
			//	Category = "Развлечения",
			//	RequiresVpn = false
			//},
			new RssFeed
			{
				Name = "Афиша",
				Url = "https://www.afisha.ru/export/rss.xml",
				Description = "События и анонсы",
				Category = "Развлечения",
				RequiresVpn = false
			},

            // 5. Региональные новости
   //         new RssFeed
			//{
			//	Name = "Фонтанка (СПб)",
			//	Url = "https://www.fontanka.ru/fontanka.rss",
			//	Description = "Новости Петербурга",
			//	Category = "Региональные",
			//	RequiresVpn = false
			//},
			//new RssFeed
			//{
			//	Name = "E1 (Екатеринбург)",
			//	Url = "https://www.e1.ru/news/rss.xml",
			//	Description = "Новости Екатеринбурга",
			//	Category = "Региональные",
			//	RequiresVpn = false
			//}
		};
		}

		public static Dictionary<string, List<RssFeed>> GetFeedsByCategory()
		{
			var feeds = GetAllFeeds();
			var result = new Dictionary<string, List<RssFeed>>();

			foreach (var feed in feeds)
			{
				if (!result.ContainsKey(feed.Category))
				{
					result[feed.Category] = new List<RssFeed>();
				}
				result[feed.Category].Add(feed);
			}

			return result;
		}

	}
}
