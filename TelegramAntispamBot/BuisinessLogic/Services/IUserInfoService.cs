using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramAntispamBot.DataAccessLayer;
using TelegramAntispamBot.DomainLayer.Models;

namespace TelegramAntispamBot.BuisinessLogic.Services
{
	public interface IUserInfoService
	{
		public UserInfo Get(long id);

		public bool TryAdd(UserInfo userInfo);

		public UserInfo FindByPullId(string pullId);

		public Task AddUserToBan(UserInfo userInfo);

		public List<UserEntity> GetAllBanedUsers();

	}
}