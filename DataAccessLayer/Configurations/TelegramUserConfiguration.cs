using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
	public class TelegramUserConfiguration : IEntityTypeConfiguration<TelegramUserEntity>
	{
		public void Configure(EntityTypeBuilder<TelegramUserEntity> builder)
		{
			builder.HasKey(u => u.UserId);

			builder.HasOne(u => u.Permissions)
				.WithOne(p => p.User)
				.HasForeignKey<TelegramPermissionsEntity>(p => p.UserId)
				.HasPrincipalKey<TelegramUserEntity>(u => u.UserId) // Явно указываем, что UserId уникален
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
