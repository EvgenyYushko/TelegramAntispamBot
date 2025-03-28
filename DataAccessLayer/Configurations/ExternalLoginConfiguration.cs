using DomainLayer.Models.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
	public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLoginEntity>
	{
		public void Configure(EntityTypeBuilder<ExternalLoginEntity> builder)
		{
			builder.HasKey(e => e.Id);

			builder.HasIndex(e => new { e.Provider, e.ProviderKey }).IsUnique();
		}
	}
}