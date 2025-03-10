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

            // Составной уникальный индекс для комбинации UserId и ChatId
            builder.HasIndex(p => new { p.UserId, p.ChatId })
                   .IsUnique();

            // Настройка связи с пользователем (многие-к-одному)
            builder.HasOne(p => p.User)
                .WithMany(u => u.Permissions) // Изменили WithOne на WithMany
                .HasForeignKey(p => p.UserId)
                .HasPrincipalKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка связи с чатом (многие-к-одному)
            builder.HasOne(p => p.Chat)
                .WithMany(u => u.Permissions) // Если у TelegramChannel есть навигационное свойство, укажите его здесь
                .HasForeignKey(p => p.ChatId)
                .HasPrincipalKey(c => c.Id) // Предполагая, что ChatId - PK в TelegramChannel
                .OnDelete(DeleteBehavior.Cascade); // Или Cascade в зависимости от требований
		}
	}
}
