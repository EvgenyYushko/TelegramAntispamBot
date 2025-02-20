using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Enumerations;

namespace DomainLayer.Repositories
{
	public interface IUserRepository
	{
		Task<HashSet<Permission>> GetUserPermissions(Guid userId);
	}
}
