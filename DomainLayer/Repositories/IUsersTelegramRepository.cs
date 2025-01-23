using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Models;
using ServiceLayer.Models;

namespace DomainLayer.Repositories
{
	public interface IUsersTelegramRepository
	{
		public UserInfo Get(long id);

		public bool TryAdd(UserInfo userInfo);

		public UserInfo FindByPullId(string pullId);

		public Task AddUserToBanList(UserInfo user);

		public List<UserBannedEntity> GetAllBanedUsers();
	}
}
