using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Models;
using ServiceLayer.Models;

namespace DomainLayer.Repositories
{
	public interface ITelegramUserRepository
	{
		public TelegramUser Get(long id);

		public bool TryAdd(TelegramUser userInfo);

		public TelegramUser FindByPullId(string pullId);

		public Task AddUserToBanList(TelegramUser user);

		public List<TelegramBannedUsersEntity> GetAllBanedUsers();
	}
}
