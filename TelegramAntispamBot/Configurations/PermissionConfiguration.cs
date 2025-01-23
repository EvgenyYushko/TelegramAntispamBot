using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelegramAntispamBot.DataAccessLayer;
using TelegramAntispamBot.DomainLayer.Models.Auth;
using TelegramAntispamBot.Enumerations;

namespace TelegramAntispamBot.Configurations
{
	public partial class PermissionConfiguration : IEntityTypeConfiguration<PermissionEntity>
	{
		public void Configure(EntityTypeBuilder<PermissionEntity> builder)
		{
			builder.HasKey(p => p.Id);

			var permissions = Enum.GetValues<Permission>()
				.Select(p => new PermissionEntity
				{
					Id = (int)p,
					Name = p.ToString()
				});

			builder.HasData(permissions);
		}
	}
}
