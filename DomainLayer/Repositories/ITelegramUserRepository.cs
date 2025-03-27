using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainLayer.Models;
using Infrastructure.Models;
using ServiceLayer.Models;

namespace DomainLayer.Repositories
{
	public interface ITelegramUserRepository
	{
		Task UpdateSuspiciousMessages(SuspiciousMessage message);
		public List<SuspiciousMessage> GetAllSuspiciousMessages();
		Task AddSuspiciousMessages(SuspiciousMessage message);
		public TelegramUser Get(long id);
		public TelegramUser GetFromLocal(long id);

		public Task<bool> TryAdd(TelegramUser userInfo);
		public Task<bool> TryAddUserExteranl(TelegramUser userInfo);
		public TelegramUserEntity GetByUserSiteId(Guid id);
		public List<Chanel> GetChatsByUser(long userId);
		public List<Chanel> GetAllChats();
		public Chanel GetChatById(long id);

		public TelegramUser FindByPullId(string pullId);

		public Task AddUserToBanList(TelegramUser user);

		public List<TelegramBannedUsersEntity> GetAllBanedUsers();

		public List<TelegramUserEntity> GetAllTelegramUsers();

		public Task UpdateTelegramUser(TelegramUser user);

		public Task UpdateLocalStorage();

		public List<TelegramChannelAdmin> GetAllAdmins();	

	}
}
