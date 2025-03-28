using System;
using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Infrastructure.Common
{
	public class BotSettings
	{
		public const long ADMIN_ID = 1231047171;
		public const string BOT_USER_NAME = "YN_AntispamBot";
		private static readonly string LINK = "https://telegramantispambot.onrender.com/User/Profile";
		public static string NoCommentWord = "лох";
		public static string inviteLink = $"https://t.me/{BOT_USER_NAME}?startgroup=true";
		private static readonly Dictionary<long, Queue<DateTime>> _userActivity = new();

		//Channels which are allowed to send messages to chat
		public static readonly List<string> ChannelsWhiteList = new()
		{
			"test"
		};

		public static readonly List<string> WhiteList = new()
		{
			"EvgenyYushko",
			"dizment008",
			"Test"
		};

		public static readonly string ChatSettingsInfo =
			"📝 <b>Инструкция по подключению защиты</b>\r\n\r\n" +
			$"1️⃣  Добавьте <a href=\"{inviteLink}\">@{BOT_USER_NAME}</a> в администраторы группы или канала. Можно через ссылку: <a href=\"{inviteLink}\">для группы</a> и для канала.\r\n\r\n" +
			"2️⃣  Выдайте права <b>Удаление сообщений</b> и <b>Блокировка пользователей</b>\r\n\r\n" +
			"⚠️ Обратите внимание:\r\n\r\n" +
			"— Бот совместим с другими ботами защитниками;\r\n" +
			"— Бот игнорирует сообщения администраторов;\r\n" +
			"— Бот удаляет сервисные сообщения о входе и выходе участников; \r\n" +
			"— Бот не может разжаловать или заблокировать администратора;\r\n\r\n\u200d" +
			"⚔🛡️ Подробная инструкция по установке. Если возникнут вопросы - не стесняйтесь писать @EvgenyYushko. Примеры спамных сообщений можно посмотреть в канале.";

		public static readonly string InfoMessage =
			"Отправлять сслыки могут только авторизированные пользователи бота в своём аккаунте";

		public static readonly string InfoMessageProfanityChecker =
			"Ваше сообщение содержит нецензурное выражение и было удалено";


		public static InlineKeyboardMarkup LinkButton = new(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithUrl(
					"Авторизоваться",
					LINK
				)
			}
		});


		//Add your telegram nickname here


		public static string StartInfo(long countUser, long countChats, long bannedUsers)
		{
			return "<b>Добро пожаловать!</b> \r\n  \r\n" +
					"\ud83e\udd16 Я — бот, который поможет сделать твои чаты чище и безопаснее. Без капчи и лишних сложностей — только умные алгоритмы и магия нейросетей.\r\n\r\n" +
					$"⚔️ Заблокировано <b>{bannedUsers}</b> спамеров\r\n" +
					"\ud83d\udd75\ufe0f\u200d Выявлено [] мошенников\r\n\r\n" +
					$"📖 Я уже обработал информацию о <b>{countUser}</b> пользователе в <b>{countChats}</b> группах. Добавь меня в группу или канал, и я начну автоматически удалять <b>рекламные сообщения</b> и блокировать <b>мошенников</b>.\r\n\r\n" +
					"С чего начнём?";
		}

		public static string GetStartBOtWelcomeMessage()
		{
			return "Спасибо что добавили меня в Ваш чат!\n\n" +
					"Я Бот-администратор, я буду делать следующее:  \r\n  \r\n" +
					"\u2705 Удалять сообщения и посты с нецензурными выражениями \r\n" +
					"\u2705 В постах удалять комментарии содержащие ссылки  \r\n" +
					"\u2705 Не удалять комментарии со ссылками от пользователей из белого списка  \r\n" +
					"\u2705 Не удалять комментарии со ссылками от каналов из белого списка  \r\n" +
					"\u2705 Отключать возможность комментирования определенных постов";
		}

		public static string GetWelcomeMessage(User user)
		{
			return $"Добро пожаловать, {user.FirstName}!\n\n" +
					"Тебя приветствует Бот-администратор, я буду делать следующее:  \r\n  \r\n" +
					"\u2705 Удалять сообщения и посты с нецензурными выражениями \r\n" +
					"\u2705 В постах удалять комментарии содержащие ссылки  \r\n" +
					"\u2705 Не удалять комментарии со ссылками от пользователей из белого списка  \r\n" +
					"\u2705 Не удалять комментарии со ссылками от каналов из белого списка  \r\n" +
					"\u2705 Отключать возможность комментирования определенных постов";
		}

		public static bool IsFlooding(long userId)
		{
			const int maxMessages = 5;
			const int timeWindowSeconds = 10;

			if (!_userActivity.ContainsKey(userId))
			{
				_userActivity[userId] = new Queue<DateTime>();
			}

			var now = DateTime.Now;
			var timestamps = _userActivity[userId];

			while (timestamps.Count > 0 && (now - timestamps.Peek()).TotalSeconds > timeWindowSeconds)
			{
				timestamps.Dequeue();
			}

			timestamps.Enqueue(now);
			return timestamps.Count > maxMessages;
		}
	}
}