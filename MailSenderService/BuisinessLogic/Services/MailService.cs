using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Infrastructure.Models;
using MailSenderService.ServiceLayer.Services;

namespace MailSenderService.BuisinessLogic.Services
{
	public class MailService : IMailService
	{
		private readonly MailOptions _mailOptions;
		private static string HTML = "<!DOCTYPE html>\r\n<html lang=\"ru\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>Антиспам Бот - Еженедельная рассылка</title>\r\n</head>\r\n<body style=\"background: #f0f4f8; margin: 0; padding: 0;\">\r\n    <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"max-width: 800px; margin: 20px auto; background: white; border-radius: 15px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);\">\r\n        <tr>\r\n            <td style=\"background: linear-gradient(135deg, #4299e1, #3182ce); padding: 2rem; text-align: center; color: white;\">\r\n                <img src=\"https://i.ibb.co/0jQ7Z0T/logo.png\" alt=\"Логотип Антиспам Бот\" style=\"width: 100px; margin-bottom: 1rem;\">\r\n                <h1 style=\"font-size: 24px; margin: 0;\">Еженедельный отчет Антиспам Бот</h1>\r\n                <p style=\"font-size: 16px; margin: 0;\">15-21 апреля 2024</p>\r\n            </td>\r\n        </tr>\r\n        <tr>\r\n            <td style=\"padding: 2rem;\">\r\n                <h2 style=\"font-size: 20px; margin-bottom: 1rem;\">Последние обновления</h2>\r\n                <div style=\"display: flex; gap: 1.5rem; margin-bottom: 2rem; padding: 1.5rem; background: #f7fafc; border-radius: 10px;\">\r\n                    <img src=\"https://i.ibb.co/0jQ7Z0T/news1.jpg\" alt=\"Новость 1\" style=\"width: 150px; height: 100px; object-fit: cover; border-radius: 8px;\">\r\n                    <div>\r\n                        <div style=\"color: #718096; font-size: 0.9em; margin-bottom: 0.5rem;\">18 апреля 2024</div>\r\n                        <h3 style=\"font-size: 18px; margin: 0;\">Новый алгоритм обнаружения фишинга</h3>\r\n                        <p style=\"font-size: 14px; margin: 0.5rem 0;\">Представляем улучшенную систему детектирования фишинговых атак с использованием ИИ...</p>\r\n                        <a href=\"#\" style=\"display: inline-block; padding: 0.8rem 1.5rem; background: #4299e1; color: white; text-decoration: none; border-radius: 5px; margin-top: 1rem;\">Читать далее</a>\r\n                    </div>\r\n                </div>\r\n            </td>\r\n        </tr>\r\n        <tr>\r\n            <td style=\"background: #2d3748; color: white; padding: 2rem; text-align: center;\">\r\n                <p style=\"font-size: 14px; margin: 0;\">\u00a9 2024 Антиспам Бот. Все права защищены.</p>\r\n                <div style=\"margin-top: 1rem;\">\r\n                    <a href=\"#\" style=\"color: white; text-decoration: none; margin: 0 0.5rem;\">Twitter</a> | \r\n                    <a href=\"#\" style=\"color: white; text-decoration: none; margin: 0 0.5rem;\">Facebook</a> | \r\n                    <a href=\"#\" style=\"color: white; text-decoration: none; margin: 0 0.5rem;\">Telegram</a>\r\n                </div>\r\n                <p style=\"margin-top: 1rem;\"><a href=\"#\" style=\"color: #a0aec0; text-decoration: none;\">Отписаться от рассылки</a></p>\r\n            </td>\r\n        </tr>\r\n    </table>\r\n</body>\r\n</html>";
		
		public MailService(MailOptions mailOptions)
		{
			_mailOptions = mailOptions;
		}

		public Task Send(string toAddress, string body, string theme, bool isBodyHtml, List<string> copyAddress)
		{
			return SendEmail(toAddress, theme, HTML, isBodyHtml, copyAddress);
		}

		private Task SendEmail(string toAddress, string subject, string body, bool isBodyHtml, List<string> copyAddress)
		{
			using (var smtpClient = new SmtpClient(_mailOptions.Host, _mailOptions.Port))
			{
				smtpClient.Credentials = new NetworkCredential(_mailOptions.SenderEmail, _mailOptions.SenderPassword);
				smtpClient.EnableSsl = _mailOptions.EnableSsl;

				using (MailMessage mailMessage = new MailMessage())
				{
					mailMessage.From = new MailAddress(_mailOptions.SenderEmail);
					mailMessage.To.Add(toAddress);

					if (copyAddress is not null)
					{
						foreach (var addres in copyAddress)
						{
							mailMessage.CC.Add(addres);
						}
					}

					//mailMessage.Bcc.Add(toAddress);
					mailMessage.IsBodyHtml = isBodyHtml;
					mailMessage.Subject = subject;
					mailMessage.Body = body;
					//mailMessage.Attachments.Add(new Attachment("test.txt"));

					smtpClient.Send(mailMessage);
					Console.WriteLine("Письмо отправлено!");
				}
			}

			return Task.CompletedTask;
		}
	}
}
