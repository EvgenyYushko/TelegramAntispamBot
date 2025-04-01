using System;
using System.Collections.Generic;
using System.Text;

namespace MailSenderService
{
	public static class HtmlHelper
	{
		public static string GetWeeklyReportHtml(
				string reportPeriod,
				int totalMessages,
				int activeChats,
				int likesReceived,
				int messageReactions,
				string userChatsHtml,
				int totalUsers,
				int botActiveChats,
				int processedMessages,
				int spamBlocked,
				string notificationSettingsUrl,
				string unsubscribeUrl)
		{
			var w = WeeklyReportGenerator.ExampleUsage();
			return w;
		}

		//public static string HTML =
		//	"<!DOCTYPE html>\r\n<html lang=\"ru\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>Ваша еженедельная статистика</title>\r\n    <style type=\"text/css\">\r\n        @media screen and (max-width: 600px) {\r\n            .responsive-table {\r\n                width: 100% !important;\r\n            }\r\n            .stat-block {\r\n                display: block !important;\r\n                width: 100% !important;\r\n                padding: 10px 0 !important;\r\n            }\r\n        }\r\n    </style>\r\n</head>\r\n<body style=\"margin: 0; padding: 0; font-family: Arial, sans-serif; background: #f5f5f5; font-size: 16px;\">\r\n    <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" bgcolor=\"#f5f5f5\">\r\n        <tr>\r\n            <td align=\"center\" style=\"padding: 20px 0;\">\r\n                <!--[if (gte mso 9)|(IE)]>\r\n                <table width=\"600\" align=\"center\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                <tr>\r\n                <td>\r\n                <![endif]-->\r\n                <table class=\"responsive-table\" width=\"600\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" bgcolor=\"#ffffff\" style=\"border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);\">\r\n                    <!-- Header -->\r\n                    <tr>\r\n                        <td style=\"padding: 30px; background: #4285f4; color: white; text-align: center; border-radius: 10px 10px 0 0;\">\r\n                            <h1 style=\"margin: 0; font-size: 28px; line-height: 1.3;\">Ваша еженедельная статистика</h1>\r\n                            <p style=\"margin: 10px 0 0; font-size: 18px;\">Отчет за период: {{ReportPeriod}}</p>\r\n                        </td>\r\n                    </tr>\r\n                    \r\n                    <!-- User Stats -->\r\n                    <tr>\r\n                        <td style=\"padding: 30px;\">\r\n                            <h2 style=\"margin: 0 0 25px; font-size: 24px; color: #333; line-height: 1.3;\">Ваша активность</h2>\r\n                            \r\n                            <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                                <tr>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 15px 0; border-bottom: 1px solid #eee; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Всего сообщений</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 28px; font-weight: bold; color: #4285f4;\">{{TotalMessages}}</p>\r\n                                    </td>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 15px 0; border-bottom: 1px solid #eee; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Активных чатов</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 28px; font-weight: bold; color: #4285f4;\">{{ActiveChats}}</p>\r\n                                    </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 15px 0; border-bottom: 1px solid #eee; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Лайков получено</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 28px; font-weight: bold; color: #4285f4;\">{{LikesReceived}}</p>\r\n                                    </td>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 15px 0; border-bottom: 1px solid #eee; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Реакций на сообщения</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 28px; font-weight: bold; color: #4285f4;\">{{MessageReactions}}</p>\r\n                                    </td>\r\n                                </tr>\r\n                            </table>\r\n                            \r\n                            <h3 style=\"margin: 30px 0 20px; font-size: 22px; color: #333; line-height: 1.3;\">Ваши чаты</h3>\r\n                            <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"font-size: 18px;\">\r\n                                <tr style=\"background: #f9f9f9;\">\r\n                                    <td style=\"padding: 15px; font-weight: bold; color: #333;\">Название чата</td>\r\n                                    <td style=\"padding: 15px; font-weight: bold; color: #333;\">Сообщений</td>\r\n                                    <td style=\"padding: 15px; font-weight: bold; color: #333;\">Активность</td>\r\n                                </tr>\r\n                                {{UserChats}}\r\n                            </table>\r\n                        </td>\r\n                    </tr>\r\n                    \r\n                    <!-- Bot Stats -->\r\n                    <tr>\r\n                        <td style=\"padding: 30px; background: #f9f9f9;\">\r\n                            <h2 style=\"margin: 0 0 25px; font-size: 24px; color: #333; line-height: 1.3;\">Статистика бота</h2>\r\n                            <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                                <tr>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 10px 0; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Всего пользователей</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 22px; font-weight: bold; color: #333;\">{{TotalUsers}}</p>\r\n                                    </td>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 10px 0; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Активных чатов</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 22px; font-weight: bold; color: #333;\">{{BotActiveChats}}</p>\r\n                                    </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 10px 0; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Обработано сообщений</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 22px; font-weight: bold; color: #333;\">{{ProcessedMessages}}</p>\r\n                                    </td>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 10px 0; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Заблокировано спама</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 22px; font-weight: bold; color: #333;\">{{SpamBlocked}}</p>\r\n                                    </td>\r\n                                </tr>\r\n                            </table>\r\n                        </td>\r\n                    </tr>\r\n                    \r\n                    <!-- Footer -->\r\n                    <tr>\r\n                        <td style=\"padding: 25px; text-align: center; background: #333; color: white; border-radius: 0 0 10px 10px;\">\r\n                            <p style=\"margin: 0; font-size: 16px;\">Это автоматическое сообщение, пожалуйста, не отвечайте на него</p>\r\n                            <p style=\"margin: 15px 0 0; font-size: 16px;\">\r\n                                <a href=\"{{NotificationSettingsUrl}}\" style=\"color: #aaa; text-decoration: none;\">Настройки уведомлений</a> | \r\n                                <a href=\"{{UnsubscribeUrl}}\" style=\"color: #aaa; text-decoration: none;\">Отписаться</a>\r\n                            </p>\r\n                        </td>\r\n                    </tr>\r\n                </table>\r\n                <!--[if (gte mso 9)|(IE)]>\r\n                </td>\r\n                </tr>\r\n                </table>\r\n                <![endif]-->\r\n            </td>\r\n        </tr>\r\n    </table>\r\n</body>\r\n</html>";

		public static string HTML_TEMPLATE_VERIFY_CODE = "<!DOCTYPE html>\r\n"
														+ "<html lang=\"ru\">\r\n"
														+ "<head>\r\n"
														+ "    <meta charset=\"UTF-8\">\r\n"
														+ "    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n"
														+ "    <title>Код регистрации</title>\r\n"
														+ "</head>\r\n"
														+ "<body style=\"background: #f0f4f8; margin: 0; padding: 0; font-family: Arial, sans-serif;\">\r\n"
														+ "    <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"max-width: 600px; margin: 20px auto; background: white; border-radius: 15px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);\">\r\n"
														+ "        <tr>\r\n"
														+ "            <td style=\"background: linear-gradient(135deg, #4CAF50, #2E7D32); padding: 2rem; text-align: center; color: white;\">\r\n"
														+ "                <h1 style=\"font-size: 24px; margin: 0;\">Антиспам Бот</h1>\r\n"
														+ "                <p style=\"font-size: 16px; margin: 0;\">Ваш код регистрации</p>\r\n"
														+ "            </td>\r\n"
														+ "        </tr>\r\n"
														+ "        <tr>\r\n"
														+ "            <td style=\"padding: 2rem; text-align: center;\">\r\n"
														+ "                <p style=\"font-size: 18px; margin-bottom: 1rem;\">Используйте этот код для подтверждения вашей регистрации:</p>\r\n"
														+ "                <p style=\"font-size: 24px; font-weight: bold; color: #4CAF50; background: #E8F5E9; padding: 15px; border-radius: 8px; display: inline-block;\">{kod}</p>\r\n"
														+ "                <p style=\"margin-top: 1.5rem; font-size: 14px; color: #555;\">Этот код действителен в течение 10 минут. Не передавайте его никому.</p>\r\n"
														+ "            </td>\r\n"
														+ "        </tr>\r\n"
														+ "        <tr>\r\n"
														+ "            <td style=\"background: #2d3748; color: white; padding: 2rem; text-align: center; border-radius: 0 0 15px 15px;\">\r\n"
														+ "                <p style=\"font-size: 14px; margin: 0;\">© 2024 Антиспам Бот. Все права защищены.</p>\r\n"
														+ "                <p style=\"margin-top: 1rem;\"><a href=\"#\" style=\"color: #a0aec0; text-decoration: none;\">Отписаться от рассылки</a></p>\r\n"
														+ "            </td>\r\n"
														+ "        </tr>\r\n"
														+ "    </table>\r\n"
														+ "</body>\r\n"
														+ "</html>";
	}

	public static class WeeklyReportGenerator
	{
		// Модель для статистики чата пользователя
		public class UserChatStats
		{
			public string ChatName { get; set; }
			public int MessageCount { get; set; }
			public string ActivityLevel { get; set; } // "Низкая", "Средняя", "Высокая"
		}

		// Генерация HTML для списка чатов пользователя
		public static string GenerateUserChatsHtml(List<UserChatStats> chats)
		{
			var sb = new StringBuilder();

			bool alternate = false;
			foreach (var chat in chats)
			{
				// Чередуем фон для лучшей читаемости
				string bgStyle = alternate ? "background: #f9f9f9;" : "";

				sb.AppendLine("<tr style=\"" + bgStyle + "\">");
				sb.AppendLine($"<td style=\"padding: 15px; color: #666;\">{chat.ChatName}</td>");
				sb.AppendLine($"<td style=\"padding: 15px; color: #666;\">{chat.MessageCount}</td>");
				sb.AppendLine($"<td style=\"padding: 15px; color: #666;\">{chat.ActivityLevel}</td>");
				sb.AppendLine("</tr>");

				alternate = !alternate;
			}

			return sb.ToString();
		}

		// Основной метод генерации HTML отчета
		public static string GenerateWeeklyReport(
			string userName,
			string reportPeriod,
			int totalMessages,
			int activeChats,
			int likesReceived,
			int messageReactions,
			List<UserChatStats> userChats,
			int totalUsers,
			int botActiveChats,
			int processedMessages,
			int spamBlocked,
			string notificationSettingsUrl,
			string unsubscribeUrl)
		{
			// Генерируем HTML для чатов пользователя
			string userChatsHtml = GenerateUserChatsHtml(userChats);

			// Загружаем HTML шаблон
			string htmlTemplate = "<!DOCTYPE html>\r\n<html lang=\"ru\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>Ваша еженедельная статистика</title>\r\n    <style type=\"text/css\">\r\n        @media screen and (max-width: 600px) {\r\n            .responsive-table {\r\n                width: 100% !important;\r\n            }\r\n            .stat-block {\r\n                display: block !important;\r\n                width: 100% !important;\r\n                padding: 10px 0 !important;\r\n            }\r\n        }\r\n    </style>\r\n</head>\r\n<body style=\"margin: 0; padding: 0; font-family: Arial, sans-serif; background: #f5f5f5; font-size: 16px;\">\r\n    <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" bgcolor=\"#f5f5f5\">\r\n        <tr>\r\n            <td align=\"center\" style=\"padding: 20px 0;\">\r\n                <!--[if (gte mso 9)|(IE)]>\r\n                <table width=\"600\" align=\"center\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                <tr>\r\n                <td>\r\n                <![endif]-->\r\n                <table class=\"responsive-table\" width=\"600\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" bgcolor=\"#ffffff\" style=\"border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);\">\r\n                    <!-- Header -->\r\n                    <tr>\r\n                        <td style=\"padding: 30px; background: #4285f4; color: white; text-align: center; border-radius: 10px 10px 0 0;\">\r\n                            <h1 style=\"margin: 0; font-size: 28px; line-height: 1.3;\">Ваша еженедельная статистика</h1>\r\n                            <p style=\"margin: 10px 0 0; font-size: 18px;\">Отчет за период: {{ReportPeriod}}</p>\r\n                        </td>\r\n                    </tr>\r\n                    \r\n                    <!-- User Stats -->\r\n                    <tr>\r\n                        <td style=\"padding: 30px;\">\r\n                            <h2 style=\"margin: 0 0 25px; font-size: 24px; color: #333; line-height: 1.3;\">Ваша активность</h2>\r\n                            \r\n                            <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                                <tr>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 15px 0; border-bottom: 1px solid #eee; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Всего сообщений</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 28px; font-weight: bold; color: #4285f4;\">{{TotalMessages}}</p>\r\n                                    </td>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 15px 0; border-bottom: 1px solid #eee; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Активных чатов</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 28px; font-weight: bold; color: #4285f4;\">{{ActiveChats}}</p>\r\n                                    </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 15px 0; border-bottom: 1px solid #eee; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Лайков получено</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 28px; font-weight: bold; color: #4285f4;\">{{LikesReceived}}</p>\r\n                                    </td>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 15px 0; border-bottom: 1px solid #eee; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Реакций на сообщения</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 28px; font-weight: bold; color: #4285f4;\">{{MessageReactions}}</p>\r\n                                    </td>\r\n                                </tr>\r\n                            </table>\r\n                            \r\n                            <h3 style=\"margin: 30px 0 20px; font-size: 22px; color: #333; line-height: 1.3;\">Ваши чаты</h3>\r\n                            <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"font-size: 18px;\">\r\n                                <tr style=\"background: #f9f9f9;\">\r\n                                    <td style=\"padding: 15px; font-weight: bold; color: #333;\">Название чата</td>\r\n                                    <td style=\"padding: 15px; font-weight: bold; color: #333;\">Сообщений</td>\r\n                                    <td style=\"padding: 15px; font-weight: bold; color: #333;\">Активность</td>\r\n                                </tr>\r\n                                {{UserChats}}\r\n                            </table>\r\n                        </td>\r\n                    </tr>\r\n                    \r\n                    <!-- Bot Stats -->\r\n                    <tr>\r\n                        <td style=\"padding: 30px; background: #f9f9f9;\">\r\n                            <h2 style=\"margin: 0 0 25px; font-size: 24px; color: #333; line-height: 1.3;\">Статистика бота</h2>\r\n                            <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                                <tr>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 10px 0; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Всего пользователей</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 22px; font-weight: bold; color: #333;\">{{TotalUsers}}</p>\r\n                                    </td>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 10px 0; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Активных чатов</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 22px; font-weight: bold; color: #333;\">{{BotActiveChats}}</p>\r\n                                    </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 10px 0; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Обработано сообщений</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 22px; font-weight: bold; color: #333;\">{{ProcessedMessages}}</p>\r\n                                    </td>\r\n                                    <td class=\"stat-block\" width=\"50%\" style=\"padding: 10px 0; display: inline-block;\">\r\n                                        <p style=\"margin: 0; font-size: 18px; color: #666;\">Заблокировано спама</p>\r\n                                        <p style=\"margin: 8px 0 0; font-size: 22px; font-weight: bold; color: #333;\">{{SpamBlocked}}</p>\r\n                                    </td>\r\n                                </tr>\r\n                            </table>\r\n                        </td>\r\n                    </tr>\r\n                    \r\n                    <!-- Footer -->\r\n                    <tr>\r\n                        <td style=\"padding: 25px; text-align: center; background: #333; color: white; border-radius: 0 0 10px 10px;\">\r\n                            <p style=\"margin: 0; font-size: 16px;\">Это автоматическое сообщение, пожалуйста, не отвечайте на него</p>\r\n                            <p style=\"margin: 15px 0 0; font-size: 16px;\">\r\n                                <a href=\"{{NotificationSettingsUrl}}\" style=\"color: #aaa; text-decoration: none;\">Настройки уведомлений</a> | \r\n                                <a href=\"{{UnsubscribeUrl}}\" style=\"color: #aaa; text-decoration: none;\">Отписаться</a>\r\n                            </p>\r\n                        </td>\r\n                    </tr>\r\n                </table>\r\n                <!--[if (gte mso 9)|(IE)]>\r\n                </td>\r\n                </tr>\r\n                </table>\r\n                <![endif]-->\r\n            </td>\r\n        </tr>\r\n    </table>\r\n</body>\r\n</html>";

			// Заменяем плейсхолдеры на реальные данные
			return htmlTemplate
				.Replace("{{UserName}}", userName)
				.Replace("{{ReportPeriod}}", reportPeriod)
				.Replace("{{TotalMessages}}", totalMessages.ToString("N0"))
				.Replace("{{ActiveChats}}", activeChats.ToString())
				.Replace("{{LikesReceived}}", likesReceived.ToString("N0"))
				.Replace("{{MessageReactions}}", messageReactions.ToString("N0"))
				.Replace("{{UserChats}}", userChatsHtml)
				.Replace("{{TotalUsers}}", totalUsers.ToString("N0"))
				.Replace("{{BotActiveChats}}", botActiveChats.ToString("N0"))
				.Replace("{{ProcessedMessages}}", processedMessages.ToString("N0"))
				.Replace("{{SpamBlocked}}", spamBlocked.ToString("N0"))
				.Replace("{{NotificationSettingsUrl}}", notificationSettingsUrl)
				.Replace("{{UnsubscribeUrl}}", unsubscribeUrl);
		}

		// Пример использования
		public static string ExampleUsage()
		{
			// Подготовка тестовых данных
			var userChats = new List<UserChatStats>
		{
			new UserChatStats { ChatName = "Технологии будущего", MessageCount = 48, ActivityLevel = "Высокая" },
			new UserChatStats { ChatName = "Криптовалюты 2024", MessageCount = 35, ActivityLevel = "Средняя" },
			new UserChatStats { ChatName = "Путешествия", MessageCount = 22, ActivityLevel = "Низкая" },
			new UserChatStats { ChatName = "Книги и литература", MessageCount = 15, ActivityLevel = "Низкая" }
		};

			// Генерация HTML отчета
			string htmlReport = GenerateWeeklyReport(
				userName: "Иван Иванов",
				reportPeriod: "15-21 апреля 2024",
				totalMessages: 127,
				activeChats: 4,
				likesReceived: 42,
				messageReactions: 89,
				userChats: userChats,
				totalUsers: 12548,
				botActiveChats: 1287,
				processedMessages: 2456321,
				spamBlocked: 24567,
				notificationSettingsUrl: "https://example.com/settings",
				unsubscribeUrl: "https://example.com/unsubscribe");

			return htmlReport;
		}
	}
}