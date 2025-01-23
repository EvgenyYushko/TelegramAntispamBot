using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainLayer.Models;
using Infrastructure.Enumerations;
using ServiceLayer.Models;

namespace DomainLayer.Repositories
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
