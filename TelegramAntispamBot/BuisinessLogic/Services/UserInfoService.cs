using System;
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
	}
}
