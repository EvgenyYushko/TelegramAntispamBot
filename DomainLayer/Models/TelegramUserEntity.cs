using System;
using System.Collections.Generic;
using DomainLayer.Models.Authorization;

namespace DomainLayer.Models
{
	public class TelegramUserEntity
	{
		public long UserId { get; set; }

		public Guid? UserSiteId { get; set; }

		public string Name { get; set; }

		public DateTime CreateDate { get; set; }

		public List<TelegramPermissionsEntity> Permissions { get; set; }

		public List<TelegramChannelAdmin> AdminInChannels { get; set; } = new();

		public List<TelegramChannel> OwnedChannels { get; set; } = new();

		public List<UserChannelMembership> ChannelMemberships { get; set; } = new();

		public List<ChatMessagesEntity> ChatMessages { get; set; } = new();

		public UserEntity? UserSite { get; set; }

		public override string ToString()
		{
			return $"UserId = {UserId}, UserSiteId={(UserSiteId is null ? "null" : UserSiteId)},  this={this.GetType().Name}";
		}
	}
}