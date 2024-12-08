using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramAntispamBot.DataAccessLayer
{
	public class BotSettings
	{
		public static string NoCommentWord = "#advert";
    
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
	}
}
