using System.Linq;
using Infrastructure.Common;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Infrastructure.Extentions
{
	public static class BotExtention
	{
		public static bool ContainsUrls(this Message message)
		{
			return (message is { Entities: { } }
					&& message.Entities.Select(e => e.Type).Contains(MessageEntityType.Url)) ||
					(message is { CaptionEntities: { } }
					&& message.CaptionEntities.Select(e => e.Type).Contains(MessageEntityType.Url));
		}

		//public static bool InWhitelist(this User user)
		//{
		//	return user is { Username: { } } &&
		//			BotSettings.WhiteList.Any(w => user.Id.ToLower().Contains(w.ToLower()));
		//}

		public static bool InChannelsWhitelist(this Chat chat)
		{
			return chat is { Username: { } } &&
					BotSettings.ChannelsWhiteList.Any(w => chat.Username.ToLower().Contains(w.ToLower()));
		}

		public static bool IsChannel(this User user)
		{
			return user is { FirstName: "Telegram" };
		}

		public static bool HasEmptyMessage(this Update update)
		{
			return
				(update.Type == UpdateType.Message &&
				update.Message?.Text == null &&
				update.Message?.Caption == null) ||
				(update.Type == UpdateType.EditedMessage &&
				update.EditedMessage?.Text == null &&
				update.EditedMessage?.Caption == null);
		}
	}
}