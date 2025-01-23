using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Enumerations;

namespace ServiceLayer.Services.Authorization
{
	public interface IPermissionService
	{
        Task<HashSet<Permission>> GetPermissionsAsync(Guid userId);
	}
}
