﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Infrastructure.Models;
using MailSenderService.ServiceLayer.Services;
using static Infrastructure.Helpers.Logger;

namespace MailSenderService.BuisinessLogic.Services
{
	public class MailService : IMailService
	{
		private readonly MailOptions _mailOptions;

		public MailService(MailOptions mailOptions)
		{
			_mailOptions = mailOptions;
		}

		public Task Send(string toAddress, string body, string theme, bool isBodyHtml, List<string> copyAddress)
		{
			return SendEmail(toAddress, theme, body, isBodyHtml, copyAddress);
		}

		private Task SendEmail(string toAddress, string subject, string body, bool isBodyHtml, List<string> copyAddress)
		{
			using (var smtpClient = new SmtpClient(_mailOptions.Host, _mailOptions.Port))
			{
				smtpClient.Credentials = new NetworkCredential(_mailOptions.SenderEmail, _mailOptions.SenderPassword);
				smtpClient.EnableSsl = _mailOptions.EnableSsl;

				using (var mailMessage = new MailMessage())
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
					Log("Письмо отправлено!");
				}
			}

			return Task.CompletedTask;
		}
	}
}