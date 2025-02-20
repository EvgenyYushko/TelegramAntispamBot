using System;
using System.Linq;
using DomainLayer.Models.Authorization;
using Infrastructure.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
	public class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
	{
		public void Configure(EntityTypeBuilder<RoleEntity> builder)
		{
			builder.HasKey(r => r.Id);

			builder.HasMany(r => r.Permissions)
				.WithMany(p => p.Roles)
				.UsingEntity<RolePermissionEntity>(
					l => l.HasOne<PermissionEntity>().WithMany().HasForeignKey(e => e.PermissionId),
					r => r.HasOne<RoleEntity>().WithMany().HasForeignKey(e => e.RoleId));

			var roles = Enum.GetValues<Role>().Select(r => new RoleEntity
			{
				// Генерация фиксированного Guid для каждой роли (можете задать свои значения)
				Id = r switch
				{
					Role.Admin => Guid.Parse("11111111-1111-1111-1111-111111111111"),
					Role.User => Guid.Parse("22222222-2222-2222-2222-222222222222"),
					Role.Tutor => Guid.Parse("33333333-3333-3333-3333-333333333333"),
					_ => Guid.NewGuid()
				},
				// Сохраняем int значение для логики сравнения
				IdRole = (int)r,
				Name = r.ToString(),
				NormalizedName = r.ToString().ToUpperInvariant(),
				ConcurrencyStamp = Guid.NewGuid().ToString()
			}).ToArray();

			builder.HasData(roles);
		}
	}
}
