using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainLayer.Repositories;
using Infrastructure.Enumerations;
using ServiceLayer.Services.Authorization;

namespace BuisinessLogic.Services.Authorization
{
	public class PermissionService : IPermissionService
	{
		private readonly IUsersAccountRepository _usersRepository;

		public PermissionService(IUsersAccountRepository usersRepository) 
		{
			_usersRepository = usersRepository;
		}

		public Task<HashSet<Permission>> GetPermissionsAsync(Guid userId)
		{
			return _usersRepository.GetUserPermissions(userId);
		}
	}
}
