using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramAntispamBot.Common
{
	public class BotSettings
	{
		public static string NoCommentWord = "лох";
    
		//Add your telegram nickname here
		public static readonly List<string> WhiteList = new()
		{
			//"EvgenyYushko",
			"Test"
		};
    
		//Channels which are allowed to send messages to chat
		public static readonly List<string> ChannelsWhiteList = new()
		{
			"test"
		};

		public static readonly string InfoMessage =
			"Если ты хочешь отправлять ссылки в комментариях канала, " +
			"то заходи в репозиторий {} и вноси себя в WhiteList (подробнее читай в файле README.md репозитория)";

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
