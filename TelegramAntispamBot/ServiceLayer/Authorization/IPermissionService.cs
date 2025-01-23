using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramAntispamBot.Enumerations;

namespace TelegramAntispamBot.ServiceLayer.Authorization
{
	public interface IPermissionService
	{
        Task<HashSet<Permission>> GetPermissionsAsync(Guid userId);
	}
}
