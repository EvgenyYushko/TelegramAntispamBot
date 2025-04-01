using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.InjectSettings;
using MailSenderService;
using MailSenderService.ServiceLayer.Services;
using Quartz;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.Jobs.Base;
using static Infrastructure.Helpers.Logger;

namespace TelegramAntispamBot.Jobs
{
	public class SendMailJob : SchedulerJob
	{
		private readonly IMailService _mailService;

		public SendMailJob(TelegramInject botClient, ITelegramUserService telegramUserService, IMailService mailService)
			: base(botClient, telegramUserService)
		{
			_mailService = mailService;
		}

		public override async Task Execute(IJobExecutionContext context)
		{
			try
			{
				var users = _telegramUserService.GetAllTelegramUsers();

				foreach (var user in users.Where(u => u.UserSite is not null))
				{
					//var copies = new List<string>() { "yushkoevgeny@gmail.com" };
					var body = HtmlHelper.GetWeeklyReportHtml("много", 2, 3, 4, 5, "cahts", 6, 7, 8, 9, "aaa", "dsds");
					Task.Run(async () => await _mailService.Send(user.UserSite.Email, body, "testTheme", isBodyHtml: true)).Wait();
				}
			}
			catch (Exception ex)
			{
				Log(ex);
			}

			await base.Execute(context);
		}
	}
}