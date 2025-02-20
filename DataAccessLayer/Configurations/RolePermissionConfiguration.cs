using System;
using System.Collections.Generic;
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
			// Карта соответствия Enum Role -> Guid
			var roleIdMap = new Dictionary<Role, Guid>
			{
				{ Role.Admin, Guid.Parse("11111111-1111-1111-1111-111111111111") },
				{ Role.User, Guid.Parse("22222222-2222-2222-2222-222222222222") },
				{ Role.Tutor, Guid.Parse("33333333-3333-3333-3333-333333333333") }
			};

			return _authorizationOptions.RolePermissions
				.SelectMany(rp => rp.Permission
					.Select(p => new RolePermissionEntity
					{
						RoleId = roleIdMap[Enum.Parse<Role>(rp.Role)],  
						PermissionId = (int)Enum.Parse<Permission>(p)
					}))
				.ToArray();
		}
	}
}
