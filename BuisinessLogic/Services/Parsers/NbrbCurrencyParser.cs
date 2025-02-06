using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Infrastructure.Helpers.FunctionsHelper;

namespace BuisinessLogic.Services.Parsers
{
	public class NbrbCurrencyParser
	{
		private const string BASE_URL = "http://www.nbrb.by/Services/XmlExRates.aspx";

		public async Task<string> ParseCurrencyRates(DateTime date)
		{
			string dateParam = date.ToString("M/d/yyyy");
			string url = $"{BASE_URL}?ondate={dateParam}";

			using (var httpClient = new HttpClient())
			{
				try
				{
					var response = await httpClient.GetStringAsync(url);
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
					return $"❌ *Ошибка!* ❌\n`{ex.Message}`";
				}
			}
		}
	}
}
