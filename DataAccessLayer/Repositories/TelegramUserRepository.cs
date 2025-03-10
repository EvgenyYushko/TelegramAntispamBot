using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainLayer.Models;
using DomainLayer.Repositories;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Models;
using Telegram.Bot.Types;
using static Infrastructure.Common.TimeZoneHelper;
using static Infrastructure.Helpers.TelegramUserHelper;

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
			var user = LocalUserStorage.FirstOrDefault(user => user.UserId == id);
			if(user == null)
			{
				var userDb = _context.TelegramUsers
					.Include(t => t.Permissions)
					.ThenInclude(p => p.Chat)
					.FirstOrDefault(u => u.UserId.Equals(id));

				user = new TelegramUser
				{
					UserId = userDb.UserId,
					Name = userDb.Name,
					CreateDate = userDb.CreateDate,
					Permissions = userDb.Permissions
						.Select(p => new TelegramPermissions()
						{
							Id = p.Id,
							UserId = p.UserId,
							ChatId = p.ChatId,
							SendLinks = p.SendLinks,
							Chanel = new Chanel()
							{
								TelegramChatId = p.Chat.Id,
								Title = p.Chat.Title
							}
						}).ToList()
				};
				LocalUserStorage.Add(user);
			}

			return user;
		}

		public TelegramUserEntity GetByUserSiteId(Guid id)
		{
			var allTgusers = GetAllTelegramUsers();
			var user = allTgusers.FirstOrDefault(u => u.UserSiteId.Equals(id));
			return user;
		}

		public async Task<bool> TryAddUserExteranl(TelegramUser userInfo)
		{
			if (LocalUserStorage.Exists(u => u.UserId == userInfo.UserId))
			{
				Console.WriteLine("Пользователь уже существует в локльном хранилище");
			}
			else
			{
				Console.WriteLine("Пользователь Не существует в локльном хранилище");
				LocalUserStorage.Add(userInfo);
			}

			var user = await _context.TelegramUsers.FirstOrDefaultAsync(u => u.UserId.Equals(userInfo.UserId));
			if (user is not null)
			{
				user.UserSiteId =  userInfo.UserSiteId;

				var userChats = _context.UserChannelMembership
					.Where(m => m.UserId == userInfo.UserId);

				var usersChatPErmisions = _context.TelegramPermissions
					.Where(p => userChats.Any(c => c.ChannelId.Equals(p.ChatId)))					;

				foreach (var per in usersChatPErmisions)
				{
					per.SendLinks = true;
				}

				Console.WriteLine($"Данный пользователь уже существует в БД");
				await _context.SaveChangesAsync();

				return false;
			}

			await AddUser(userInfo.UserId, userInfo.Name, userInfo.UserSiteId);
			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<bool> TryAdd(TelegramUser userInfo)
		{
			if (LocalUserStorage.Exists(u => u.UserId == userInfo.UserId))
			{
				Console.WriteLine("Пользователь уже существует в локльном хранилище");
			}
			else
			{
				Console.WriteLine("Пользователь Не существует в локльном хранилище");
				LocalUserStorage.Add(userInfo);
			}

			if (_context.TelegramUsers.Any(u => u.UserId.Equals(userInfo.UserId)))
			{
				Console.WriteLine($"Данный пользователь уже существует в БД");

				if (!_context.UserChannelMembership.Any(u => u.UserId.Equals(userInfo.UserId) && u.ChannelId.Equals(userInfo.Chanel.TelegramChatId)))
				{
					//Console.WriteLine($"Данный пользователь уже существует в канале {userInfo.Chanel.TelegramChatId}");
					await TryAddChanel(userInfo);
				}
				else
				{
					await UpdateChatAdmins(userInfo);
				}
				// есть в БД но нету в этом чате
				await _context.SaveChangesAsync();
				return false;
			}

			// пользака нету в БД - создадим
			await AddUserWithPErmissions(userInfo.User.Id, userInfo.User.Username, userInfo.Chanel.TelegramChatId);
			await TryAddChanel(userInfo);

			await _context.SaveChangesAsync();

			return true;
		}

		private async Task TryAddChanel(TelegramUser userInfo)
		{
			await AddChanel(userInfo);
			await UpdateMemberShip(userInfo);
		}

		private async Task AddChanel(TelegramUser userInfo)
		{
			if (!_context.TelegramChanel.Any(u => u.Id.Equals(userInfo.Chanel.TelegramChatId)))
			{
				var creator = _context.TelegramUsers.FirstOrDefault(u => u.UserId.Equals(userInfo.Chanel.Creator.UserId));
				if (creator is null && userInfo.Chanel.CreatorId != userInfo.UserId)
				{
					await AddUserWithPErmissions(userInfo.Chanel.Creator.UserId, userInfo.Chanel.Creator.Name, userInfo.Chanel.TelegramChatId);
				}

				var chanel = new TelegramChannel
				{
					Id = userInfo.Chanel.TelegramChatId,
					Title = userInfo.Chanel.Title,
					ChatType = userInfo.Chanel.ChatType,
					CreatorId = userInfo.Chanel.CreatorId
				};
				await _context.TelegramChanel.AddAsync(chanel);

				if (userInfo.Chanel.AdminsMembers.Count > 0)
				{
					foreach (var admin in userInfo.Chanel.AdminsMembers)
					{
						var adm = _context.TelegramUsers.FirstOrDefault(u => u.UserId.Equals(admin.UserId));
						if (adm is null)
						{
							await AddUserWithPErmissions(admin.UserId, admin.Name, userInfo.Chanel.TelegramChatId);
						}
						await _context.TelegramChannelAdmin.AddAsync(new TelegramChannelAdmin
						{
							ChannelId = userInfo.Chanel.TelegramChatId,
							UserId = admin.UserId
						});
					}
				}
			}
		}

		private async Task AddUserWithPErmissions(long userId, string userName, long chatId)
		{
			await AddUser(userId, userName);
			await _context.TelegramPermissions.AddAsync(new TelegramPermissionsEntity
			{
				UserId = userId,
				ChatId = chatId,
				SendLinks = false
			});
		}

		private async Task AddUser(long userId, string userName, Guid userSiteId = default)
		{
			await _context.TelegramUsers.AddAsync(new TelegramUserEntity
			{
				UserId = userId,
				Name = userName,
				CreateDate = DateTimeNow,
				UserSiteId = userSiteId
			});
		}

		private async Task UpdateChatAdmins(TelegramUser userInfo)
		{
			var admins = GetAdmins(userInfo);
			if (admins.Count > 0)
			{
				var oldAdmins = _context.TelegramChannelAdmin.Where(a => a.ChannelId.Equals(userInfo.Chanel.TelegramChatId)).ToList();
				_context.TelegramChannelAdmin.RemoveRange(oldAdmins);

				await _context.TelegramChannelAdmin.AddRangeAsync(admins);
			}
		}

		private static List<TelegramChannelAdmin> GetAdmins(TelegramUser userInfo)
		{
			return userInfo.Chanel.AdminsMembers.Select(admin => new TelegramChannelAdmin
			{
				ChannelId = userInfo.Chanel.TelegramChatId,
				UserId = admin.UserId
			}).ToList();
		}

		private async Task UpdateMemberShip(TelegramUser userInfo)
		{
			var membership = new UserChannelMembership
			{
				UserId = userInfo.UserId,
				ChannelId = userInfo.Chanel.TelegramChatId,
				JoinDate = DateTimeNow
			};
			await _context.UserChannelMembership.AddAsync(membership);
		}

		public List<Chanel> GetAllChats()
		{
			var chats = _context.UserChannelMembership
				.Include(u => u.Channel)
				.ThenInclude(c => c.Creator)
				.AsNoTracking()
				.DistinctBy(c => c.ChannelId);

			return chats.Select(u =>

				new Chanel
				{
					CreatorId = u.Channel.CreatorId,
					Creator = new TelegramUser()
					{
						UserId = u.Channel.Creator.UserId,
						Name = u.Channel.Creator.Name
					},
					ChatType = u.Channel.ChatType,
					TelegramChatId = u.Channel.Id,
					Title = u.Channel.Title,
					AdminsIds = u.Channel.Admins.Select(a => a.UserId).ToList()
				}
			).ToList();
		}

		public List<Chanel> GetChatsByUser(long userId)
		{
			var chats = _context.UserChannelMembership
				.Include(u => u.Channel)
				.AsNoTracking()
				.Where(m => m.UserId == userId);

			return chats.Select(u =>

				new Chanel
				{
					CreatorId = u.Channel.CreatorId,
					ChatType = u.Channel.ChatType,
					TelegramChatId = u.Channel.Id,
					Title = u.Channel.Title,
					AdminsIds = u.Channel.Admins.Select(a => a.UserId).ToList()
				}
			).ToList();
		}

		public Chanel GetChatById(long id)
		{
			var chat = _context.TelegramChanel
				.Include(c => c.Members)
				.ThenInclude(c => c.User)
				.Include(c => c.Admins)
				.ThenInclude(m => m.User)
				.First(m => m.Id.Equals(id));

			return new Chanel
			{
				CreatorId = chat.CreatorId,
				Creator = new TelegramUser()
				{
					UserId = chat.Creator.UserId,
					Name = chat.Creator.Name
				},
				ChatType = chat.ChatType,
				TelegramChatId = chat.Id,
				Title = chat.Title,
				AdminsIds = chat.Admins.Select(a => a.UserId).ToList(),
				AdminsMembers = chat.Admins
					.Select(m => m.User)
					.Select(m => new TelegramUser
					{
						UserId = m.UserId,
						Name = m.Name,
						CreateDate = DateTime.Now
					}).ToList(),
				Members = chat.Members
					.Select(m => m.User)
					.Select(m => new TelegramUser
					{
						UserId = m.UserId,
						Name = m.Name,
						CreateDate = DateTime.Now
					}).ToList()
			};
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
					telegramUser.Permissions = userDbo.Permissions
						.Select(p => new TelegramPermissions()
						{
							Id = p.Id,
							UserId = p.UserId,
							ChatId = p.ChatId,
							SendLinks = p.SendLinks
						}).ToList();
					continue;
				}

				var user = new TelegramUser
				{
					UserId = userDbo.UserId,
					Name = userDbo.Name,
					CreateDate = userDbo.CreateDate,
					Permissions = userDbo.Permissions
						.Select(p => new TelegramPermissions()
						{
							Id = p.Id,
							UserId = p.UserId,
							ChatId = p.ChatId,
							SendLinks = p.SendLinks
						}).ToList()
				};

				LocalUserStorage.Add(user);
			}

			return Task.CompletedTask;
		}

		public async Task UpdateTelegramUser(TelegramUser user)
		{
			var tgPer = _context.TelegramUsers.FirstOrDefault(p => p.UserId.Equals(user.UserId));
			tgPer.Permissions = user.Permissions
				.Select(p => new TelegramPermissionsEntity()
				{
					Id = p.Id,
					UserId = p.UserId,
					ChatId = p.ChatId,
					SendLinks = p.SendLinks
				}).ToList();
				

			await _context.SaveChangesAsync();
		}
	}
}
