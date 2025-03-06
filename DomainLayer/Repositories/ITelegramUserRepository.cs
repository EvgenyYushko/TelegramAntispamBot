using System.Collections.Generic;
using System.Threading.Tasks;
using DomainLayer.Models;
using Infrastructure.Models;
using ServiceLayer.Models;

namespace DomainLayer.Repositories
{
	public interface ITelegramUserRepository
	{
		public TelegramUser Get(long id);

		public Task<bool> TryAdd(TelegramUser userInfo);

		public List<Chanel> GetChatsByUser(long userId);
		public List<Chanel> GetAllChats();
		public Chanel GetChatById(long id);

		public TelegramUser FindByPullId(string pullId);

		public Task AddUserToBanList(TelegramUser user);

		public List<TelegramBannedUsersEntity> GetAllBanedUsers();

		public List<TelegramUserEntity> GetAllTelegramUsers();

		public Task UpdateTelegramUser(TelegramUser user);

		public Task UpdateLocalStorage();

	}
}
