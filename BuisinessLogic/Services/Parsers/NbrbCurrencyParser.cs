using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Infrastructure.Helpers.FunctionsHelper;
using static Infrastructure.Common.TimeZoneHelper;

namespace BuisinessLogic.Services.Parsers
{
	public class NbrbCurrencyParser
	{
		private const string BASE_URL = "https://www.nbrb.by/Services/XmlExRates.aspx";

		public static async Task<WebProxy> GetRandomProxyAsync()
		{
			string[] proxyList = new string[]
			{
				"http://140.238.24.231",
			};

			foreach (var proxyUrl in proxyList)
			{
				try
				{
					var proxy = new WebProxy(proxyUrl);
					var httpClientHandler = new HttpClientHandler { Proxy = proxy, UseProxy = true };
					var httpClient = new HttpClient(httpClientHandler);
        
					Console.WriteLine($"🔄 Пробую прокси: {proxyUrl}");
					var response = await httpClient.GetStringAsync("http://www.nbrb.by/Services/XmlExRates.aspx?ondate=2/12/2025");
        
					Console.WriteLine("✅ Успешный ответ через прокси!");
					Console.WriteLine(response);
					return proxy;
					break;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"❌ Ошибка с прокси {proxyUrl}: {ex.Message}");
				}
			}

			return null;
		}

		public async Task<string> ParseCurrencyRates()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

			var proxy = await GetRandomProxyAsync();

			if (proxy == null)
			{
				return "❌ Не удалось получить рабочий прокси.";
			}

			Console.WriteLine("Address="+proxy.Address);

			var handler = new HttpClientHandler
			{
				Proxy = proxy,
				UseProxy = true,
				ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
			};

			using (var httpClient = new HttpClient(handler))
			{
				try
				{
					var date = DateTimeNow;
					string dateParam = date.ToString("M/d/yyyy");
					string url = $"{BASE_URL}?ondate={dateParam}";

					Console.WriteLine(url);
					httpClient.Timeout = new TimeSpan(0, 0, 1, 40);
					httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
					var response = await httpClient.GetStringAsync(url);
					Console.WriteLine($"Ответ от сервера:\n{response}"); // 🔴 Добавляем вывод ответа в лог
					var xdoc = XDocument.Parse(response);
					var dateElement = xdoc.Root.Element("Date")?.Value;

					var sb = new StringBuilder();
					// Добавляем смайлы и Markdown-разметку
					sb.AppendLine($"💰 *Курсы валют НБ РБ на {dateElement ?? date.ToShortDateString()}*:\n");

					var currencies = xdoc.Root.Elements("Currency")
						.Select(v => new
						{
							Code = v.Element("CharCode")?.Value,
							Name = v.Element("Name")?.Value,
							Rate = DecimalParse(v.Element("Rate")?.Value, out decimal rate) ? rate : 0m,
							Scale = int.TryParse(v.Element("Scale")?.Value, out int scale) ? scale : 1
						})
						.Where(c => !string.IsNullOrEmpty(c.Code))
						.ToList();

					// Стилизация для EUR и USD
					foreach (var currency in currencies.Where(c => c.Code.Equals("EUR") || c.Code.Equals("USD")))
					{
						var emoji = currency.Code switch
						{
							"EUR" => "🇪🇺",
							"USD" => "🇺🇸",
							"RUB" => "🇷🇺",
							"CNY" => "🇨🇳",
							"GBP" => "🇬🇧",
							_ => "💵"
						};

						sb.AppendLine($"{emoji} *{currency.Code}* ({currency.Name})");
						sb.AppendLine($"`{currency.Scale} {currency.Code}` = *{currency.Rate} BYN*\n");
					}

					// Добавляем разделитель и время обновления
					sb.AppendLine("—————————————");
					sb.AppendLine($"🕒 _Обновлено: {DateTime.Now:HH:mm}_");

					return sb.ToString();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
					return $"❌ *Ошибка!* ❌\n`{ex.Message}`";
				}
			}
		}
	}
}
