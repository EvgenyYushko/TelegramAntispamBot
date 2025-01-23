using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramAntispamBot.DomainLayer.Models;
using TelegramAntispamBot.Enumerations;

namespace TelegramAntispamBot.DomainLayer.Repositories
{
	public interface IUsersAccountRepository
	{
		Task Add(UserAccount user);
		Task<UserAccount> GetByEmail(string email);
		Task<UserAccount> GetByUserName(string userName);
		Task<UserAccount> GetById(Guid id);
		Task<HashSet<Permission>> GetUserPermissions(Guid userId);
	}
}
