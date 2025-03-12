using System.Linq;
using System.Threading.Tasks;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Helpers
{
	internal static class UserRepositoryHelper
	{
		public static Task<TelegramUserEntity> GetUser(this ApplicationDbContext context, long userId)
		{
			return context.TelegramUsers.AsNoTracking().FirstOrDefaultAsync(u => u.UserId.Equals(userId));
		}

		public static bool UserInChanel(this ApplicationDbContext context, long userId, long chanelId)
		{
			return context.UserChannelMembership.Any(u => u.UserId.Equals(userId) && u.ChannelId.Equals(chanelId));
		}

		public static bool ExistsChanel(this ApplicationDbContext context, long chanelId)
		{
			return context.TelegramChanel.Any(u => u.Id.Equals(chanelId));
		}
	}
}
