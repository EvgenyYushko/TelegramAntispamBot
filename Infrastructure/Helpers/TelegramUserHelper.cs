using System;
using System.Collections.Generic;
using Infrastructure.Models;
using Telegram.Bot.Types;

namespace Infrastructure.Helpers
{
	public static class TelegramUserHelper
	{
		public static void ResetPull(this TelegramUser user)
		{
			user.PullModel.PollMessageId = 0;
			user.PullModel.Message = null;
			user.PullModel.PullId = null;
		}

		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector)
		{
			var seenKeys = new HashSet<TKey>();
			foreach (var element in source)
			{
				if (seenKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}

		public static string GetMsgText(this Update update)
		{
			return update.Message.Text ?? update.Message.Caption;
		}
	}
}