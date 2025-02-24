using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Infrastructure.Common
{
	public class BotSettings
	{
		public static string NoCommentWord = "лох";
		private static string LINK = "https://telegramantispambot.onrender.com/User/Profile";

		//Add your telegram nickname here
		public static readonly List<string> WhiteList = new()
		{
			"EvgenyYushko",
			"dizment008",
			"Test"
		};

		//Channels which are allowed to send messages to chat
		public static readonly List<string> ChannelsWhiteList = new()
		{
			"test"
		};

		public static readonly string InfoMessage =	$"Отправлять сслыки могут только авторизированные пользователи бота в своём аккаунте";

		public static InlineKeyboardMarkup LinkButton = new InlineKeyboardMarkup(new[]
			{
				new[]
				{
					InlineKeyboardButton.WithUrl(
						text: "Авторизоваться",
						url: LINK
					)
				}
			});

		public static readonly string InfoMessageProfanityChecker =
			"Ваше сообщение содержит нецензурное выражение и было удалено";

		public static string GetWelcomeMessage(User user) =>
			$"Добро пожаловать, {user.FirstName}!\n\n" +
			"Тебя приветствует Бот-администратор, я буду делать следующее:  \r\n  \r\n" +
			"\u2705 Удалять сообщения и посты с нецензурными выражениями \r\n" +
			"\u2705 В постах удалять комментарии содержащие ссылки  \r\n" +
			"\u2705 Не удалять комментарии со ссылками от пользователей из белого списка  \r\n" +
			"\u2705 Не удалять комментарии со ссылками от каналов из белого списка  \r\n" +
			"\u2705 Отключать возможность комментирования определенных постов";
	}
}
