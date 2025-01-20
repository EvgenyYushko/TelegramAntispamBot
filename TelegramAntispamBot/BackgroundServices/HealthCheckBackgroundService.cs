using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TelegramAntispamBot.DomainLayer.Models;

namespace TelegramAntispamBot.BackgroundServices
{
	public class HealthCheckBackgroundService : BackgroundService
	{
		private const string HEALTH_URL = "/health";
		private readonly AppOptions _appSettings;
		private readonly HttpClient _httpClient;

		public HealthCheckBackgroundService(IOptions<AppOptions> appSettings)
		{
			_appSettings = appSettings.Value;
            _httpClient = new HttpClient();
		}

		/// <inheritdoc />
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var response = await _httpClient.GetAsync(_appSettings.Domain + HEALTH_URL, stoppingToken);
					if (response.IsSuccessStatusCode)
					{
						var content = await response.Content.ReadAsStringAsync(stoppingToken);
						Console.WriteLine(content);
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
			_httpClient.Dispose();
			base.Dispose();
		}
	}
}
