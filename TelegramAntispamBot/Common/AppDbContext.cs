using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TelegramAntispamBot.Common
{

	public class AppDbContext : DbContext
	{
		public DbSet<MessageUser> Messages { get; set; }

		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<MessageUser>(entity =>
			{
				entity.HasKey(m => m.Id);
				entity.Property(m => m.UserId).IsRequired();
				entity.Property(m => m.MessageText).HasMaxLength(500);
			});
		}
	}

	public class MessageUser
	{
		public int Id { get; set; } // Первичный ключ
		public long UserId { get; set; }
		public string UserName { get; set; }
		public string MessageText { get; set; }
		public DateTime SentAt { get; set; }
	}
}
