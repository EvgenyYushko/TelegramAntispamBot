using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Enumerations;
using ServiceLayer.Models;

namespace DomainLayer.Repositories
{
	public interface IUserRepository
	{
		Task Add(UserAccount user);
		Task<UserAccount> GetByEmail(string email);
		Task<UserAccount> GetByUserName(string userName);
		Task<UserAccount> GetById(Guid id);
		Task<HashSet<Permission>> GetUserPermissions(Guid userId);
	}
}
