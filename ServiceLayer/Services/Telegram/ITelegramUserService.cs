using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Models;
using ServiceLayer.Models;
using Telegram.Bot.Types;

namespace ServiceLayer.Services.Telegram
{
	public interface ITelegramUserService
	{
		public TelegramUser Get(long id);

		public Task<bool> TryAdd(TelegramUser userInfo);

		public Task<bool> TryAddUserExteranl(TelegramUser userInfo);

		public TelegramUser GetByUserSiteId(Guid id);

		public List<Chanel> GetChatsByUser(long userId);

		public List<Chanel> GetAllChats();

		public Chanel GetChatById(long id);

		public TelegramUser FindByPullId(string pullId);

		public Task AddUserToBan(TelegramUser userInfo);

		public List<TelegramBannedUsersEntity> GetAllBanedUsers();

		public List<TelegramUser> GetAllTelegramUsers();

		public Task UpdateTelegramUser(TelegramUser user);

		public Task UpdateLocalStorage();

		public Task<bool> InWhitelist(long id);

		public Task<bool> CheckReputation(Message message);
	}
}