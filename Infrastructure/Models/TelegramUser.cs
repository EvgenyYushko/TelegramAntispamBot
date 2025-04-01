using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace Infrastructure.Models
{
	public class TelegramUser
	{
		public long UserId { get; set; }

		public Guid UserSiteId { get; set; } = default;

		public UserAccount UserSite { get; set; }
		
		public string Name { get; set; }

		public DateTime CreateDate { get; set; }

		public List<TelegramPermissions> Permissions { get; set; } = new();

		public User User { get; set; }

		public PullModel PullModel { get; set; } = new();

		public Chanel Chanel { get; set; } = new();

		public override bool Equals(object obj)
		{
			return obj is TelegramUser u && u.User.Id == User.Id;
		}

		public override string ToString()
		{
			return $"UserId={UserId}, Name={Name} UserSiteId={UserSiteId} this={this.GetType().Name}";
		}
	}

	public class Chanel
	{
		public long TelegramChatId { get; set; }

		public long CreatorId { get; set; }

		public string Title { get; set; }

		public string ChatType { get; set; }

		public TelegramUser Creator { get; set; }

		public ChatMember ChatMember { get; set; }

		public List<TelegramUser> AdminsMembers { get; set; } = new();

		public List<long> AdminsIds { get; set; } = new();

		public List<TelegramUser> Members { get; set; } = new();

		public ChatPermissions ChatPermission { get; set; } = new();

		public override string ToString()
		{
			return
				$"TelegramChatId = {TelegramChatId}, Title = {Title}, CreatorId = {CreatorId}, AdminsIds ={string.Join(", ", AdminsIds)}";
		}
	}
}