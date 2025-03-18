using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainLayer.Models;
using DomainLayer.Repositories;
using Infrastructure.Models;
using ServiceLayer.Models;
using ServiceLayer.Services.Telegram;
using Telegram.Bot.Types;

namespace BuisinessLogic.Services.Telegram
{
	public class TelegramUserService : ITelegramUserService
	{
		private readonly ITelegramUserRepository _usersRepository;

		public TelegramUserService(ITelegramUserRepository usersRepository)
		{
			_usersRepository = usersRepository;
		}

		public Task UpdateSuspiciousMessages(SuspiciousMessageDto message)
		{
			return _usersRepository.UpdateSuspiciousMessages(new SuspiciousMessage()
			{
				Id = message.Id,
				Text = message.Text,
				CreatedAt = message.CreatedAt,
				IsSpamByGemini = message.IsSpamByGemini,
				IsSpamByMl = message.IsSpamByMl,
				IsSpamByUser = message.IsSpamByUser,
				NeedsManualReview = message.NeedsManualReview,
				Probability = message.Probability
			});
		}

		public Task AddSuspiciousMessages(SuspiciousMessageDto message)
		{
			return _usersRepository.AddSuspiciousMessages(new SuspiciousMessage()
			{
				Text = message.Text,
				CreatedAt = message.CreatedAt,
				IsSpamByGemini = message.IsSpamByGemini,
				IsSpamByMl = message.IsSpamByMl,
				IsSpamByUser = message.IsSpamByUser,
				NeedsManualReview = message.NeedsManualReview,
				Probability = message.Probability
			});
		}

		public List<SuspiciousMessageDto> GetAllSuspiciousMessages()
		{
			var msgs = _usersRepository.GetAllSuspiciousMessages();

			return msgs.Select(m => new SuspiciousMessageDto()
			{
				Text = m.Text,
				IsSpamByUser = m.IsSpamByUser,
				NeedsManualReview = m.NeedsManualReview,
				Probability = m.Probability,
				CreatedAt = m.CreatedAt,
				Id = m.Id,
				IsSpamByGemini = m.IsSpamByGemini,
				IsSpamByMl = m.IsSpamByMl
			}).ToList();	
		}

		public TelegramUser Get(long id)
		{
			return _usersRepository.Get(id);
		}

		public TelegramUser GetFromLocal(long id)
		{
			return _usersRepository.GetFromLocal(id);
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
		
		public Task<bool> TryAddUserExteranl(TelegramUser userInfo)
		{
			return _usersRepository.TryAddUserExteranl(userInfo);
		}

		public List<Chanel> GetChatsByUser(long userId)
		{
			return _usersRepository.GetChatsByUser(userId);
		}

		public List<Chanel> GetAllChats()
		{
			return _usersRepository.GetAllChats();
		}

		public Chanel GetChatById(long id)
		{
			return _usersRepository.GetChatById(id);
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

		public TelegramUser GetByUserSiteId(Guid id)
		{
			var tgUser = _usersRepository.GetByUserSiteId(id);
			if (tgUser == null)
			{
				return null;
			}
			return new TelegramUser
			{
				UserId = tgUser.UserId,
				Name = tgUser.Name,
				CreateDate = tgUser.CreateDate,
				Permissions = tgUser.Permissions?.Select(p => new TelegramPermissions()
				{
					Id = p.Id,
					UserId = p.UserId,
					SendLinks = p.SendLinks
				})
				.ToList(),
				UserSiteId = id
			};
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
					Permissions = u.Permissions.Select(p => new TelegramPermissions()
					{
						Id = p.Id,
						UserId = p.UserId,
						SendLinks = p.SendLinks
					})
					.ToList()
				})
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

		public async Task<bool> InWhitelist(long id)
		{
			var user = _usersRepository.Get(id);
			return user.Permissions.First().SendLinks;
		}

		public async Task<bool> CheckReputation(Message message)
		{
			var user = _usersRepository.GetFromLocal(message.From.Id);
			if (user == null)
			{
				user = new()
				{
					User = message.From
				};
				await _usersRepository.TryAdd(user);
			}

			user.PullModel.CountFoul++;

			if (user.PullModel.CountFoul >= 3)
			{
				return false;
			}

			return true;
		}		
	}
}
