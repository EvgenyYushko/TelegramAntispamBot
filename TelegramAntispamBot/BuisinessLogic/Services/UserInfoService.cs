using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramAntispamBot.DataAccessLayer;
using TelegramAntispamBot.DataAccessLayer.Repositories;
using TelegramAntispamBot.DomainLayer.Models;

namespace TelegramAntispamBot.BuisinessLogic.Services
{
	public class UserInfoService : IUserInfoService
	{
		private UsersRepository _usersRepository { get; }

		public UserInfoService(UsersRepository usersRepository)
		{
			_usersRepository = usersRepository;
		}

		public UserInfo Get(long id)
		{
			return _usersRepository.Get(id);
		}

		public bool TryAdd(UserInfo userInfo)
		{
			var res = _usersRepository.TryAdd(userInfo);
			if (res)
			{
				Console.WriteLine("User already exist to DB");
			}

			return res;
		}

		public UserInfo FindByPullId(string pullId)
		{
			return _usersRepository.FindByPullId(pullId);
		}

		public async Task AddUserToBan(UserInfo userInfo)
		{
			await _usersRepository.AddUserToBanList(userInfo);
		}

		public List<UserEntity> GetAllBanedUsers()
		{
			return _usersRepository.GetAllBanedUsers();
		}
	}
}
