using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainLayer.Repositories;
using Infrastructure.Models;
using ServiceLayer.Models;
using static Infrastructure.Common.TimeZoneHelper;

namespace DataAccessLayer.Repositories
{
	public class TelegramUserRepository : ITelegramUserRepository
	{
		private readonly ApplicationDbContext _context;

		private List<TelegramUser> UsersInfo { get; set; }

		public TelegramUserRepository(ApplicationDbContext context)
		{
			_context = context;
			UsersInfo = new();
		}

		public TelegramUser Get(long id)
		{
			foreach (var user in UsersInfo)
			{
				if (user.User.Id == id)
				{
					return user;
				}
			}

			return null;
		}

		public bool TryAdd(TelegramUser userInfo)
		{
			if (UsersInfo.Contains(userInfo))
			{
				Console.WriteLine("Данный пользователь уже существует в БД");
				return false;
			}

			UsersInfo.Add(userInfo);
			return true;
		}

		public TelegramUser FindByPullId(string pullId) 
		{
			return UsersInfo.Cast<TelegramUser>().FirstOrDefault(u => u.PullModel.PullId == pullId);
		}

		public async Task AddUserToBanList(TelegramUser user)
		{
			var userEntity = new TelegramBannedUsersEntity()
			{
				Id = user.User.Id,
				UserName = user.User.Username,
				DateAdd = DateTimeNow
			};
			await _context.BanedUsers.AddAsync(userEntity);
			await _context.SaveChangesAsync();
		}

		public List<TelegramBannedUsersEntity> GetAllBanedUsers()
		{
			var s = _context.BanedUsers.ToList();
			return s;
		}
	}
}
