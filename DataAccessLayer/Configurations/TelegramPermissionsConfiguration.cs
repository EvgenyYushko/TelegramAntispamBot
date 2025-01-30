using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
	public class TelegramPermissionsConfiguration : IEntityTypeConfiguration<TelegramPermissionsEntity>
	{
		public void Configure(EntityTypeBuilder<TelegramPermissionsEntity> builder)
		{
			builder.HasKey(u => u.Id);

			builder.HasIndex(p => p.UserId).IsUnique(); // UserId должен быть уникальным

			builder.HasOne(p => p.User)
				.WithOne(u => u.Permissions)
				.HasForeignKey<TelegramPermissionsEntity>(p => p.UserId)
				.HasPrincipalKey<TelegramUserEntity>(u => u.UserId) // UserId в TelegramUserEntity является уникальным ключом
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
