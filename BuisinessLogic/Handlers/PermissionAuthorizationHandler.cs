using System;
using System.Linq;
using System.Threading.Tasks;
using BuisinessLogic.Services.Authorization;
using Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.Services.Authorization;

namespace BuisinessLogic.Handlers
{
	public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory)
		{
			_serviceScopeFactory = serviceScopeFactory;
		}

		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
		{
			var userId = context.User.Claims.FirstOrDefault(c => c.Type == CustomClaims.UserId);

			if (userId is null || !Guid.TryParse(userId.Value, out var id))
			{
				return;
			}

			using var scope = _serviceScopeFactory.CreateScope();

			var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

			var permissions = await permissionService.GetPermissionsAsync(id);

			if (permissions.Intersect(requirement.Permissions).Any())
			{
				context.Succeed(requirement);
			}
		}
	}
}
