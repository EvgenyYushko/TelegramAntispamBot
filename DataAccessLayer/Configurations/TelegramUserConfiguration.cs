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

		}
	}
}
