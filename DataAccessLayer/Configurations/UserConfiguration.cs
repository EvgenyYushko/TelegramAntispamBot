using DomainLayer.Models.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
	public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
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
