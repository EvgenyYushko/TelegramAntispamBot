using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramAntispamBot.DomainLayer.Models;

namespace TelegramAntispamBot.ServiceLayer.Services
{
	public interface IUserInfoService
	{
		public UserInfo Get(long id);

		public bool TryAdd(UserInfo userInfo);

		public UserInfo FindByPullId(string pullId);

		public Task AddUserToBan(UserInfo userInfo);

		public List<UserBannedEntity> GetAllBanedUsers();

	}
}