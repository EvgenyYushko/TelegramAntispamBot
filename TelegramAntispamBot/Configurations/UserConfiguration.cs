using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelegramAntispamBot.DataAccessLayer;
using TelegramAntispamBot.DomainLayer.Models.Auth;

namespace TelegramAntispamBot.Configurations
{
	public partial class UserConfiguration : IEntityTypeConfiguration<UserEntity>
	{
		public void Configure(EntityTypeBuilder<UserEntity> builder)
		{
			builder.HasKey(u => u.Id);

			builder.HasMany(u => u.Roles)
				.WithMany(r => r.Users)
				.UsingEntity<UserRoleEntity>(
					l => l.HasOne<RoleEntity>().WithMany().HasForeignKey(r => r.RoleId),
					r => r.HasOne<UserEntity>().WithMany().HasForeignKey(u => u.UserId));
		}
	}
}
