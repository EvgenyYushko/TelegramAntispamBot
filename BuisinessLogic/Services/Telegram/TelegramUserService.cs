using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainLayer.Repositories;
using Infrastructure.Models;
using ServiceLayer.Models;
using ServiceLayer.Services.Telegram;

namespace BuisinessLogic.Services.Telegram
{
	public class TelegramUserService : ITelegramUserService
	{
		private readonly ITelegramUserRepository _usersRepository;

		public TelegramUserService(ITelegramUserRepository usersRepository)
		{
			_usersRepository = usersRepository;
		}

		public TelegramUser Get(long id)
		{
			return _usersRepository.Get(id);
		}

		public async Task<bool> TryAdd(TelegramUser userInfo)
		{
			var res = await _usersRepository.TryAdd(userInfo);
			if (res)
			{
				Console.WriteLine("User already exist");
			}

			return res;
		}

		public TelegramUser FindByPullId(string pullId)
		{
			return _usersRepository.FindByPullId(pullId);
		}

		public async Task AddUserToBan(TelegramUser userInfo)
		{
			await _usersRepository.AddUserToBanList(userInfo);
		}

		public List<TelegramBannedUsersEntity> GetAllBanedUsers()
		{
			return _usersRepository.GetAllBanedUsers();
		}

		public List<TelegramUser> GetAllTelegramUsers()
		{
			var tgUsers = _usersRepository.GetAllTelegramUsers();
			return tgUsers
				.Select(u => new TelegramUser
				{
					UserId = u.UserId,
					Name = u.Name,
					CreateDate = u.CreateDate,
					Permissions = new TelegramPermissions
					{
						Id = u.Permissions.Id,
						UserId = u.Permissions.UserId,
						SendLinks = u.Permissions.SendLinks
					}
				} )
				.ToList();
		}

		public Task UpdateTelegramUser(TelegramUser user)
		{
			return _usersRepository.UpdateTelegramUser(user);
		}

		/// <inheritdoc />
		public Task UpdateLocalStorage()
		{
			return _usersRepository.UpdateLocalStorage();
		}
	}
}
