using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
	public class ChatPermissionsEntityConfiguration : IEntityTypeConfiguration<ChatPermissionsEntity>
	{
		public void Configure(EntityTypeBuilder<ChatPermissionsEntity> builder)
		{
			builder.ToTable("ChatPermissions"); // Опционально - имя таблицы в БД

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.IsRequired()
				.ValueGeneratedOnAdd();

			builder.Property(x => x.SendNews)
				.IsRequired();

			// Настройка связи с TelegramChannel
			builder.HasOne(x => x.Chat)
				.WithOne(x => x.ChatPermissions) // Убедитесь, что в TelegramChannel есть коллекция Permissions
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade); // или другой подходящий вариант
		}
	}
}
