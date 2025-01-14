using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramAntispamBot.DomainLayer.Models;

namespace TelegramAntispamBot.DataAccessLayer.Repositories
{
	public class UsersRepository
	{
		private readonly ApplicationDbContext _context;

		private List<UserInfo> UsersInfo { get; set; }

		public UsersRepository(ApplicationDbContext context)
		{
			_context = context;
			UsersInfo = new();
		}

		public UserInfo Get(long id)
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

		public bool TryAdd(UserInfo userInfo)
		{
			if (UsersInfo.Contains(userInfo))
			{
				Console.WriteLine("Данный пользователь уже существует в БД");
				return false;
			}

			UsersInfo.Add(userInfo);
			return true;
		}

		public UserInfo FindByPullId(string pullId) 
		{
			return UsersInfo.Cast<UserInfo>().FirstOrDefault(u => u.PullModel.PullId == pullId);
		}

		public async Task AddUserToBanList(UserInfo user)
		{
			var userEntity = new UserEntity()
			{
				Id = user.User.Id,
				UserName = user.User.Username,
				DateAdd = DateTime.Now
			};
			await _context.BanedUsers.AddAsync(userEntity);
			await _context.SaveChangesAsync();
		}
	}
}
