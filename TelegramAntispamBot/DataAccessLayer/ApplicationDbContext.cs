using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TelegramAntispamBot.Configurations;
using TelegramAntispamBot.DomainLayer.Models;
using TelegramAntispamBot.DomainLayer.Models.Auth;

namespace TelegramAntispamBot.DataAccessLayer
{
	public class ApplicationDbContext: DbContext
	{
		private readonly AuthorizationOptions _authorizationOptions;

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IOptions<AuthorizationOptions> authOptions) : base(options) 
		{
			_authorizationOptions = authOptions.Value;    
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
			modelBuilder.ApplyConfiguration(new RolePermissionConfiguration(_authorizationOptions));
		}

		public DbSet<UserBannedEntity> BanedUsers { get; set; } = null!;

		public DbSet<UserEntity> Users { get; set; }

		public DbSet<RoleEntity> Roles { get; set; }

		public DbSet<PermissionEntity> Permissions { get; set; }

		public DbSet<UserRoleEntity> UserRoleEntity { get; set; }
	}
}
