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
					l => l.HasOne(ur => ur.Role).WithMany().HasForeignKey(ur => ur.RoleId),  
					r => r.HasOne(ur => ur.User).WithMany().HasForeignKey(ur => ur.UserId));

			builder.HasMany(u => u.ExternalLogins)
				.WithOne(e => e.User)
				.HasForeignKey(e => e.UserId);
		}
	}
}
