using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainLayer.Models;
using DomainLayer.Repositories;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Models;
using static Infrastructure.Common.TimeZoneHelper;

namespace DataAccessLayer.Repositories
{
	public class TelegramUserRepository : ITelegramUserRepository
	{
		private readonly ApplicationDbContext _context;

		public TelegramUserRepository(ApplicationDbContext context)
		{
			_context = context;
			LocalUserStorage = new();
		}

		private List<TelegramUser> LocalUserStorage { get; set; }

		public TelegramUser Get(long id)
		{
			return LocalUserStorage.FirstOrDefault(user => user.User.Id == id);
		}

		public async Task<bool> TryAdd(TelegramUser userInfo)
		{
			if (LocalUserStorage.Exists(u => u.UserId == userInfo.UserId))
			{
				Console.WriteLine("Пользователь уже существует в локльном хранилище");
				return false;
			}
			
			Console.WriteLine("Пользователь Не существует в локльном хранилище");
			LocalUserStorage.Add(userInfo);

			if (_context.TelegramUsers.Any(u => u.UserId.Equals(userInfo.User.Id)))
			{
				Console.WriteLine("Данный пользователь уже существует в БД");
				await UpdateLocalStorage();
				return false;
			}

			Console.WriteLine("Пользователь Не существует в БД");
			await _context.TelegramUsers.AddAsync(new TelegramUserEntity
			{
				UserId = userInfo.User.Id,
				Name = userInfo.User.Username,
				CreateDate = DateTimeNow
			});
			
			await _context.TelegramPermissions.AddAsync(new TelegramPermissionsEntity
			{
				UserId = userInfo.User.Id,
				SendLinks = false
			});

			await _context.SaveChangesAsync();

			return true;
		}

		public TelegramUser FindByPullId(string pullId) 
		{
			return LocalUserStorage.FirstOrDefault(u => u.PullModel.PullId == pullId);
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
			return _context.BanedUsers.ToList();
		}

		public List<TelegramUserEntity> GetAllTelegramUsers()
		{
			return _context.TelegramUsers
				.Include(t => t.Permissions)
				.AsNoTracking()
				.ToList();
		}

		//public TelegramUser GetTelegramUser(long id)
		//{
		//	var user = _context.TelegramUsers
		//		.Include(t => t.Permissions)
		//		.AsNoTracking()
		//		.FirstOrDefault(u => u.UserId == id);

		//	var userDto = new TelegramUser
		//	{
		//		UserId = user.UserId,
		//		Name = user.Name,
		//		CreateDate = user.CreateDate,
		//		Permissions = new TelegramPermissions()
		//		{
		//			UserId = user.UserId,
		//			Id = user.Permissions.Id,
		//			SendLinks = user.Permissions.SendLinks
		//		}
		//	};

		//	if (LocalUserStorage.Contains(userDto))
		//	{
		//		var telegramUser = LocalUserStorage.Find(u => u.UserId == userDto.UserId);
		//		telegramUser.Permissions.SendLinks = userDto.Permissions.SendLinks;
		//	}
		//	else
		//	{
		//		LocalUserStorage.Add(userDto);
		//	}

		//	return userDto;
		//}

		public Task UpdateLocalStorage()
		{
			var usersDbo = GetAllTelegramUsers();
			foreach (var userDbo in usersDbo)
			{
				var telegramUser = LocalUserStorage.Find(u => u.UserId == userDbo.UserId);
				if (telegramUser is not null)
				{
					telegramUser.Permissions.SendLinks = userDbo.Permissions.SendLinks;
					continue;
				}

				var user = new TelegramUser
				{
					UserId = userDbo.UserId,
					Name = userDbo.Name,
					CreateDate = userDbo.CreateDate,
					Permissions = new TelegramPermissions
					{
						UserId = userDbo.UserId,
						Id = userDbo.Permissions.Id,
						SendLinks = userDbo.Permissions.SendLinks
					}
				};

				LocalUserStorage.Add(user);
			}

			return Task.CompletedTask;
		}

		public async Task UpdateTelegramUser(TelegramUser user)
		{
			var tgPer = _context.TelegramPermissions.FirstOrDefault(p => p.UserId.Equals(user.UserId));
			tgPer.SendLinks = user.Permissions.SendLinks;

			await _context.SaveChangesAsync();
		}
	}
}
