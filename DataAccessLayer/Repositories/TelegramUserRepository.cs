using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer.Helpers;
using DomainLayer.Models;
using DomainLayer.Repositories;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Models;
using static Infrastructure.Common.TimeZoneHelper;
using static Infrastructure.Helpers.TelegramUserHelper;
using static Infrastructure.Helpers.Logger;

namespace DataAccessLayer.Repositories
{
	public class TelegramUserRepository : ITelegramUserRepository
	{
		private readonly ApplicationDbContext _context;

		public TelegramUserRepository(ApplicationDbContext context)
		{
			_context = context;
			LocalUserStorage = new List<TelegramUser>();
		}

		private List<TelegramUser> LocalUserStorage { get; }

		public Task<List<SuspiciousMessage>> GetAllSuspiciousMessages()
		{
			return _context.SuspiciousMessages
				.AsNoTracking()
				.ToListAsync();
		}

		public async Task AddSuspiciousMessages(SuspiciousMessage message)
		{
			await _context.SuspiciousMessages.AddAsync(message);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteReviewedSuspiciousMessages()
		{
			var messagesToDelete = _context.SuspiciousMessages
				.Where(s => !s.NeedsManualReview && s.IsSpamByUser != null);

			_context.SuspiciousMessages.RemoveRange(messagesToDelete);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateSuspiciousMessages(SuspiciousMessage message)
		{
			var msg = await _context.SuspiciousMessages
				.FirstOrDefaultAsync(m => m.Id.Equals(message.Id));

			Log(msg);

			msg.IsSpamByUser = message.IsSpamByUser;
			msg.NeedsManualReview = message.NeedsManualReview;

			Log(msg);

			await _context.SaveChangesAsync();
		}

		public TelegramUser Get(long id)
		{
			var userDb = _context.TelegramUsers
				.Include(t => t.Permissions)
				.ThenInclude(p => p.Chat)
				.AsNoTracking()
				.FirstOrDefault(u => u.UserId.Equals(id));

			return new TelegramUser
			{
				UserId = userDb.UserId,
				Name = userDb.Name,
				CreateDate = userDb.CreateDate,
				Permissions = userDb.Permissions
					.Select(p => new TelegramPermissions
					{
						Id = p.Id,
						UserId = p.UserId,
						ChatId = p.ChatId,
						SendLinks = p.SendLinks,
						Chanel = new Chanel
						{
							TelegramChatId = p.Chat.Id,
							Title = p.Chat.Title
						}
					}).ToList(),
				UserSiteId = userDb.UserSiteId ?? default
			};
		}

		public TelegramUser GetFromLocal(long id)
		{
			var user = LocalUserStorage.FirstOrDefault(user => user.UserId == id);
			if (user == null)
			{
				var userDb = _context.TelegramUsers
					.Include(t => t.Permissions)
					.ThenInclude(p => p.Chat)
					.AsNoTracking()
					.FirstOrDefault(u => u.UserId.Equals(id));

				user = new TelegramUser
				{
					UserId = userDb.UserId,
					Name = userDb.Name,
					CreateDate = userDb.CreateDate,
					Permissions = userDb.Permissions
						.Select(p => new TelegramPermissions
						{
							Id = p.Id,
							UserId = p.UserId,
							ChatId = p.ChatId,
							SendLinks = p.SendLinks,
							Chanel = new Chanel
							{
								TelegramChatId = p.Chat.Id,
								Title = p.Chat.Title
							}
						}).ToList(),
					UserSiteId = userDb.UserSiteId ?? default
				};
				LocalUserStorage.Add(user);
			}

			return user;
		}

		public TelegramUserEntity GetByUserSiteId(Guid id)
		{
			return GetAllTelegramUsers().FirstOrDefault(u => u.UserSiteId.Equals(id));
		}

		public async Task<bool> TryAddUserExteranl(TelegramUser userInfo)
		{
			var userLocal = LocalUserStorage.FirstOrDefault(u => u.UserId == userInfo.UserId);
			if (userLocal is not null)
			{
				Log("Пользователь уже существует в локльном хранилище - Удалим");
				//var indexUser = LocalUserStorage.IndexOf(userLocal);
				LocalUserStorage.Clear();
			}

			var user = await _context.GetUser(userInfo.UserId, false);
			if (user is not null)
			{
				Log(user);
				Log(userInfo);
				user.UserSiteId = userInfo.UserSiteId;
				Log(user);

				var userChats = _context.UserChannelMembership
					.Where(m => m.UserId == userInfo.UserId);

				var usersChatPErmisions = _context.TelegramPermissions
					.Where(p => userChats.Any(c => c.ChannelId.Equals(p.ChatId) && c.UserId.Equals(p.UserId)));

				await usersChatPErmisions.ForEachAsync(p => p.SendLinks = true);

				Log("Данный пользователь уже существует в БД");
				await _context.SaveChangesAsync();

				return true;
			}

			Log("TryAddUserExteranl");
			Log(userInfo);
			await AddUser(userInfo.UserId, userInfo.Name, userInfo.UserSiteId);
			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<bool> TryAdd(TelegramUser userInfo)
		{
			if (LocalUserStorage.Exists(u => u.UserId == userInfo.UserId))
			{
				Log("Пользователь уже существует в локльном хранилище");
			}
			else
			{
				Log("Пользователь Не существует в локльном хранилище");
				LocalUserStorage.Add(userInfo);
			}

			if (await _context.GetUser(userInfo.UserId) is not null)
			{
				Log("Данный пользователь уже существует в БД");

				// Пользака нету в текущем канале
				if (!_context.UserInChanel(userInfo.UserId, userInfo.Chanel.TelegramChatId))
				{
					//Log($"Данный пользователь уже существует в канале {userInfo.Chanel.TelegramChatId}");
					await TryAddChanel(userInfo);
				}
				else
				{
					await UpdateChatAdmins(userInfo);
				}

				// есть в БД но нету в этом чате
				await AddOrUpdateTelegrammPermission(userInfo.UserId, userInfo.Chanel.TelegramChatId);
				await _context.SaveChangesAsync();
				return false;
			}

			// пользака нету в БД - создадим
			await AddUserWithPermissions(userInfo.User.Id, userInfo.User.Username, userInfo.Chanel.TelegramChatId);
			await TryAddChanel(userInfo);

			await _context.SaveChangesAsync();

			return true;
		}

		private async Task TryAddChanel(TelegramUser userInfo)
		{
			await AddChanel(userInfo);
			await UpdateMemberShip(userInfo.UserId, userInfo.Chanel.TelegramChatId);
		}

		private async Task AddChanel(TelegramUser userInfo)
		{
			var chat = userInfo.Chanel;
			var creator = chat.Creator;

			if (!_context.ExistsChanel(chat.TelegramChatId))
			{
				var creatorUser = await _context.GetUser(creator.UserId);
				if (creatorUser is null && chat.CreatorId != userInfo.UserId)
				{
					Log("Begin AddUserWithPErmissions");
					await AddUserWithPermissions(creator.UserId, creator.Name, chat.TelegramChatId);
				}

				await AddTelegramChannel(chat);
				await AddTelegramChannelPermissions(chat);

				if (chat.AdminsMembers.Count > 0)
				{
					await AddAdminsToChat(userInfo);
				}
			}
		}

		private async Task AddUserWithPermissions(long userId, string userName, long chatId)
		{
			await AddUser(userId, userName);
			await AddOrUpdateTelegrammPermission(userId, chatId);
		}

		private async Task UpdateChatAdmins(TelegramUser userInfo)
		{
			var chat = userInfo.Chanel;
			if (chat.AdminsMembers.Count > 0)
			{
				DropChatAdmins(userInfo.Chanel.TelegramChatId);
				await AddAdminsToChat(userInfo);
			}
		}

		private void DropChatAdmins(long chatId)
		{
			var oldAdmins = _context.TelegramChannelAdmin.Where(a => a.ChannelId.Equals(chatId)).ToList();
			_context.TelegramChannelAdmin.RemoveRange(oldAdmins);
		}

		private async Task AddAdminsToChat(TelegramUser userInfo)
		{
			var chat = userInfo.Chanel;

			foreach (var admin in chat.AdminsMembers)
			{
				var adm = await _context.GetUser(admin.UserId);
				if (adm is null && admin.UserId != userInfo.UserId)
				{
					await AddUserWithPermissions(admin.UserId, admin.Name, chat.TelegramChatId);
					await UpdateMemberShip(admin.UserId, chat.TelegramChatId);
				}

				await AddAdmin(userInfo, admin);
			}
		}

		private async Task AddAdmin(TelegramUser userInfo, TelegramUser admin)
		{
			await _context.TelegramChannelAdmin.AddAsync(new TelegramChannelAdmin
			{
				ChannelId = userInfo.Chanel.TelegramChatId,
				UserId = admin.UserId
			});
		}

		private async Task AddTelegramChannel(Chanel chat)
		{
			var chanel = new TelegramChannel
			{
				Id = chat.TelegramChatId,
				Title = chat.Title,
				ChatType = chat.ChatType,
				CreatorId = chat.CreatorId
			};
			await _context.TelegramChanel.AddAsync(chanel);
		}

		private async Task AddTelegramChannelPermissions(Chanel chat)
		{
			var chatPerns = new ChatPermissionsEntity
			{
				ChatId = chat.TelegramChatId,
				SendNews = true
			};
			await _context.ChatPermissions.AddAsync(chatPerns);
		}

		private async Task AddOrUpdateTelegrammPermission(long userId, long chatId)
		{
			Log("AddOrUpdateTelegrammPermission");
			var pers = _context.TelegramPermissions
				.AsNoTracking()
				.FirstOrDefault(p => p.UserId.Equals(userId) && p.ChatId.Equals(chatId));

			if (pers is null)
			{
				await _context.TelegramPermissions.AddAsync(new TelegramPermissionsEntity
				{
					UserId = userId,
					ChatId = chatId,
					SendLinks = true
				});
			}
		}

		private async Task AddUser(long userId, string userName, Guid? userSiteId = null)
		{
			await _context.TelegramUsers.AddAsync(new TelegramUserEntity
			{
				UserId = userId,
				Name = userName,
				CreateDate = DateTimeNow,
				UserSiteId = userSiteId
			});
		}

		private async Task UpdateMemberShip(long userId, long chatId)
		{
			var membership = new UserChannelMembership
			{
				UserId = userId,
				ChannelId = chatId,
				JoinDate = DateTimeNow
			};
			await _context.UserChannelMembership.AddAsync(membership);
		}

		public List<Chanel> GetAllChats()
		{
			var chats = _context.UserChannelMembership
				.Include(u => u.Channel)
				.ThenInclude(c => c.Creator)
				.Include(c => c.Channel.ChatPermissions)
				.AsNoTracking()
				.DistinctBy(c => c.ChannelId);

			return chats.Select(u =>
				new Chanel
				{
					CreatorId = u.Channel.CreatorId,
					Creator = new TelegramUser
					{
						UserId = u.Channel.Creator.UserId,
						Name = u.Channel.Creator.Name
					},
					ChatType = u.Channel.ChatType,
					TelegramChatId = u.Channel.Id,
					Title = u.Channel.Title,
					AdminsIds = u.Channel.Admins.Select(a => a.UserId).ToList(),
					ChatPermission = new Infrastructure.Models.ChatPermissions()
					{
						Id = u.Channel.ChatPermissions.Id,
						ChatId = u.Channel.ChatPermissions.ChatId,
						SendNews = u.Channel.ChatPermissions.SendNews,
						AllNewsCronExpression = u.Channel.ChatPermissions.AllNewsCronExpression,
						SendCurrency = u.Channel.ChatPermissions.SendCurrency,
						CurrencyCronExpression = u.Channel.ChatPermissions.CurrencyCronExpression,
						SendHabr = u.Channel.ChatPermissions.SendHabr,
						HabrCronExpression = u.Channel.ChatPermissions.HabrCronExpression,
						SendOnliner = u.Channel.ChatPermissions.SendOnliner,
						OnlinerCronExpression = u.Channel.ChatPermissions.OnlinerCronExpression
					}
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
				.Include(c => c.Creator)
				.Include(c => c.Admins)
				.ThenInclude(m => m.User)
				.Include(c => c.ChatPermissions)
				//.AsNoTracking()
				.First(m => m.Id.Equals(id));

			Log($"chat.Creator={chat.Creator}");
			Log($"chat.Admins={chat.Admins}");
			Log($"chat.Members={chat.Members}");

			return new Chanel
			{
				CreatorId = chat.CreatorId,
				Creator = new TelegramUser
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
						CreateDate = m.CreateDate
					}).ToList(),
				Members = chat.Members
					.Select(m => m.User)
					.Select(m => new TelegramUser
					{
						UserId = m.UserId,
						Name = m.Name,
						CreateDate = m.CreateDate
					}).ToList(),
				ChatPermission = new Infrastructure.Models.ChatPermissions
				{
					Id = chat.ChatPermissions.Id,
					ChatId = chat.ChatPermissions.ChatId,
					SendNews = chat.ChatPermissions.SendNews,
					AllNewsCronExpression = chat.ChatPermissions.AllNewsCronExpression,
					SendCurrency = chat.ChatPermissions.SendCurrency,
					CurrencyCronExpression = chat.ChatPermissions.CurrencyCronExpression,
					SendHabr = chat.ChatPermissions.SendHabr,
					HabrCronExpression = chat.ChatPermissions.HabrCronExpression,
					SendOnliner = chat.ChatPermissions.SendOnliner,
					OnlinerCronExpression = chat.ChatPermissions.OnlinerCronExpression
				}
			};
		}

		public TelegramUser FindByPullId(string pullId)
		{
			return LocalUserStorage.FirstOrDefault(u => u.PullModel.PullId == pullId);
		}

		public async Task AddUserToBanList(TelegramUser user)
		{
			var userEntity = new TelegramBannedUsersEntity
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
			return _context.BanedUsers
				.AsNoTracking()
				.ToList();
		}

		public List<TelegramUserEntity> GetAllTelegramUsers()
		{
			return _context.TelegramUsers
				.Include(t => t.Permissions)
				.Include(t => t.UserSite)
				.AsNoTracking()
				.ToList();
		}

		public Task UpdateLocalStorage()
		{
			var usersDbo = GetAllTelegramUsers();
			foreach (var userDbo in usersDbo)
			{
				var telegramUser = LocalUserStorage.Find(u => u.UserId == userDbo.UserId);
				if (telegramUser is not null)
				{
					telegramUser.Permissions = userDbo.Permissions
						.Select(p => new TelegramPermissions
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
						.Select(p => new TelegramPermissions
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
				.Select(p => new TelegramPermissionsEntity
				{
					Id = p.Id,
					UserId = p.UserId,
					ChatId = p.ChatId,
					SendLinks = p.SendLinks
				}).ToList();


			await _context.SaveChangesAsync();
		}

		public List<TelegramChannelAdmin> GetAllAdmins()
		{
			return _context.TelegramChannelAdmin
				.AsNoTracking()
				.ToList();
		}

		public async Task UpdateChatPermissions(Infrastructure.Models.ChatPermissions chatPermissions)
		{
			var per = await _context.ChatPermissions
				.FirstOrDefaultAsync(c => c.ChatId.Equals(chatPermissions.ChatId));

			per.SendNews = chatPermissions.SendNews;
			per.AllNewsCronExpression = chatPermissions.AllNewsCronExpression;
			per.SendCurrency = chatPermissions.SendCurrency;
			per.CurrencyCronExpression= chatPermissions.CurrencyCronExpression;
			per.SendHabr = chatPermissions.SendHabr;
			per.HabrCronExpression= chatPermissions.HabrCronExpression;
			per.SendOnliner= chatPermissions.SendOnliner;
			per.OnlinerCronExpression= chatPermissions.OnlinerCronExpression;

			await _context.SaveChangesAsync();
		}
	}
}