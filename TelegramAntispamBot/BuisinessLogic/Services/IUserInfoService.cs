using System.Threading.Tasks;
using TelegramAntispamBot.DomainLayer.Models;

namespace TelegramAntispamBot.BuisinessLogic.Services
{
	public interface IUserInfoService
	{
		public UserInfo Get(long id);

		public bool TryAdd(UserInfo userInfo);

		public UserInfo FindByPullId(string pullId);

		public Task AddUserToBan(UserInfo userInfo);
	}
}