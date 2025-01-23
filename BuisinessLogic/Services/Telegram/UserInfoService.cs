using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainLayer.Repositories;
using Infrastructure.Models;
using ServiceLayer.Models;
using ServiceLayer.Services.Telegram;

namespace BuisinessLogic.Services.Telegram
{
	public class UserInfoService : IUserInfoService
	{
		private readonly IUsersTelegramRepository _usersRepository;

		public UserInfoService(IUsersTelegramRepository usersRepository)
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

		public List<UserBannedEntity> GetAllBanedUsers()
		{
			return _usersRepository.GetAllBanedUsers();
		}
	}
}
