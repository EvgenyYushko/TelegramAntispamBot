using System;
using System.Linq;
using DomainLayer.Models.Authorization;
using Infrastructure.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
	public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermissionEntity>
	{
		private readonly AuthorizationOptions _authorizationOptions;

		public RolePermissionConfiguration(AuthorizationOptions authOptions)
		{
			_authorizationOptions = authOptions;
		}

		public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
		{
			builder.HasKey(r => new { r.RoleId, r.PermissionId });

			builder.HasData(ParseRolePermissions());
		}

		private RolePermissionEntity[] ParseRolePermissions()
		{
			return _authorizationOptions.RolePermissions
				.SelectMany(rp => rp.Permission
					.Select(p => new RolePermissionEntity
					{
						RoleId = (int)Enum.Parse<Role>(rp.Role),
						PermissionId = (int)Enum.Parse<Permission>(p)
					}))
				.ToArray();
		}
	}
}
