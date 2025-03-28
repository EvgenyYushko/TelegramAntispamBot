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

			builder.HasOne(u => u.UserSite)
				.WithOne() // Один к одному
				.HasForeignKey<TelegramUserEntity>(u => u.UserSiteId) // Внешний ключ
				.IsRequired(false) // Связь необязательная
				.OnDelete(DeleteBehavior.SetNull); // При удалении UserEntity поле UserSiteId становится null
		}
	}
}