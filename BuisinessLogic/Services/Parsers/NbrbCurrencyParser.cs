using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

namespace BuisinessLogic.Services.Parsers
{
	public class NbrbCurrencyParser
	{
		private const string BASE_URL = "https://www.nbrb.by/Services/XmlExRates.aspx";
		// Классы для десериализации JSON
		public record CurrencyRate(
			int Cur_ID,
			string Date,
			string Cur_Abbreviation,
			int Cur_Scale,
			string Cur_Name,
			decimal Cur_OfficialRate
		);

		public async Task<string> ParseCurrencyRates()
		{
			// Основной код
			using (var httpClient = new HttpClient())
			{
				try
				{
					const string BASE_URL = "https://api.nbrb.by/exrates/rates";
					var requestDate = DateTime.Now;

					// Формируем URL запроса
					var url = $"{BASE_URL}?periodicity=0";
					Console.WriteLine($"Request URL: {url}");

					// Настройка HttpClient
					httpClient.Timeout = TimeSpan.FromSeconds(10);
					httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
					httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Render/1.0 (+https://render.com)");

					// Выполнение запроса
					var response = await httpClient.GetStringAsync(url);
					Console.WriteLine("Raw response received");

					// Десериализация JSON
					var currencies = JsonSerializer.Deserialize<List<CurrencyRate>>(response);
					if (currencies == null || !currencies.Any())
						throw new Exception("Empty response from API");

					// Получаем актуальную дату из первого элемента
					var rateDate = DateTime.TryParse(currencies[0].Date, out var date)
						? date.ToShortDateString()
						: requestDate.ToShortDateString();

					var sb = new StringBuilder();
					sb.AppendLine($"💰 *Курсы валют НБ РБ на {rateDate}*:\n");

					// Фильтр и сортировка валют
					var selectedCurrencies = currencies
						.Where(c => new[] { "EUR", "USD", "RUB", "CNY", "GBP" }.Contains(c.Cur_Abbreviation))
						.OrderBy(c => c.Cur_Abbreviation)
						.ToList();

					// Формирование сообщения
					foreach (var currency in selectedCurrencies)
					{
						var emoji = currency.Cur_Abbreviation switch
						{
							"EUR" => "🇪🇺",
							"USD" => "🇺🇸",
							"RUB" => "🇷🇺",
							"CNY" => "🇨🇳",
							"GBP" => "🇬🇧",
							_ => "💵"
						};

						sb.AppendLine($"{emoji} *{currency.Cur_Abbreviation}* ({currency.Cur_Name})");
						sb.AppendLine($"`{currency.Cur_Scale} {currency.Cur_Abbreviation}` = *{currency.Cur_OfficialRate:0.0000} BYN*\n");
					}

					sb.AppendLine("—————————————");
					sb.AppendLine($"🕒 _Обновлено: {DateTime.Now:HH:mm}_");

					return sb.ToString();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex}");
					return $"❌ *Ошибка!* ❌\n`{ex.Message}`";
				}
			}
		}
	}
}
