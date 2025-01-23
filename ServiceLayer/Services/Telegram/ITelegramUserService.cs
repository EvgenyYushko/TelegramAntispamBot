using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Models;
using ServiceLayer.Models;

namespace ServiceLayer.Services.Telegram
{
	public interface ITelegramUserService
	{
		public TelegramUser Get(long id);

		public bool TryAdd(TelegramUser userInfo);

		public TelegramUser FindByPullId(string pullId);

		public Task AddUserToBan(TelegramUser userInfo);

		public List<TelegramBannedUsersEntity> GetAllBanedUsers();

	}
}