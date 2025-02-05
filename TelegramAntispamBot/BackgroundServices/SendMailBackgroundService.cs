using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MailSenderService.ServiceLayer.Services;
using Microsoft.Extensions.Hosting;

namespace TelegramAntispamBot.BackgroundServices
{
	public class SendMailBackgroundService : BackgroundService
	{
		private readonly IMailService _mailService;

		public SendMailBackgroundService(IMailService mailService)
		{
			_mailService = mailService;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					//await _mailService.Send("jeka-krut@mail.ru", "test", "testTheme", isBodyHtml:true, copyAddress: new List<string>() { "yushkoevgeny@gmail.com" });
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}

				await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
			}
		}
	}
}
