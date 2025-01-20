using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Hosting;

namespace TelegramAntispamBot.BackgroundServices
{
	public class HealthCheckBackgroundService : BackgroundService
	{
		private const string URL_SITE = "https://telegramantispambot.onrender.com";

		private const string HealthUrl = "/health";
		private readonly HttpClient _httpClient;

		public HealthCheckBackgroundService()
		{
            _httpClient = new HttpClient();
		}

		/// <inheritdoc />
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var response = await _httpClient.GetAsync(URL_SITE + HealthUrl, stoppingToken);
					if (response.IsSuccessStatusCode)
					{
						var content = await response.Content.ReadAsStringAsync(stoppingToken);
						Console.WriteLine(content);
						Console.WriteLine("Teeesssst");
					}
					else
					{
						Console.WriteLine($"Health check failed: {response.StatusCode}");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error in health check: {ex.Message}");
				}

				await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
			}
		}

		public override void Dispose()
		{
			//_httpClient.Dispose();
			base.Dispose();
		}
	}
}
