using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace DomainLayer.Models
{
	public class TelegramChannel
	{
		public long Id { get; set; } // ID чата/канала в Telegram
		public string Title { get; set; }        // Название
		public string ChatType { get; set; }     // "channel", "group", "supergroup"
		public DateTime AddedDate { get; set; } = DateTime.UtcNow;

		// Связи
		public long CreatorId { get; set; }      // Создатель (пользователь бота)
		public TelegramUserEntity Creator { get; set; }
		public List<TelegramChannelAdmin> Admins { get; set; } = new();
		public List<UserChannelMembership> Members { get; set; } = new();
		public List<TelegramPermissionsEntity> Permissions { get; set; } = new();
	}
}
