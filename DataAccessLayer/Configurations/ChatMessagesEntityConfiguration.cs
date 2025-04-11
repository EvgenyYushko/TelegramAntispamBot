using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
	public class ChatMessagesEntityConfiguration : IEntityTypeConfiguration<ChatMessagesEntity>
	{
		public void Configure(EntityTypeBuilder<ChatMessagesEntity> builder)
		{
			builder.ToTable("ChatMessages"); // Название таблицы в БД

			builder.HasKey(x => x.Id); // Указываем первичный ключ

			// Настройка связи с TelegramChannel
			builder.HasOne(x => x.Chat)            // У сообщения есть один чат
			   .WithMany(x => x.ChatMessages)      // У чата много сообщений
			   .HasForeignKey(x => x.ChatId)   // Внешний ключ
			   .OnDelete(DeleteBehavior.Cascade);  

			// Настройка связи с TelegramUserEntity
			builder.HasOne(x => x.User)
				   .WithMany(x => x.ChatMessages) // или .WithMany(u => u.Messages)
				   .HasForeignKey(x => x.UserId)
				   .OnDelete(DeleteBehavior.Cascade);

			// Настройка индексов для быстрого поиска
			builder.HasIndex(x => x.ChatId);
			builder.HasIndex(x => x.UserId);
			builder.HasIndex(x => x.CreatedAt);

			// Ограничение длины текста (если не используется nvarchar(MAX))
			builder.Property(x => x.Text)
				   .HasMaxLength(4096); // Telegram допускает до 4096 символов в сообщении
		}
	}
}
