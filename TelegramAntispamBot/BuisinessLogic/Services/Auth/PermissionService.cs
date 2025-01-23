using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramAntispamBot.DomainLayer.Repositories;
using TelegramAntispamBot.Enumerations;
using TelegramAntispamBot.ServiceLayer.Authorization;

namespace TelegramAntispamBot.BuisinessLogic.Services.Auth
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
