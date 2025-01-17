using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TelegramAntispamBot.DataAccessLayer
{
	public class ApplicationDbContext: DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

		public DbSet<UserEntity> BanedUsers { get; set; } = null!;
	}

	public class UserEntity
	{
		public long Id { get; set; }

		public string UserName { get; set; }

		public DateTime DateAdd { get; set; }

		public override string ToString() => UserName;
	}
}
