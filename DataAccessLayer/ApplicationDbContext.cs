using System;
using DataAccessLayer.Configurations;
using DomainLayer.Models;
using DomainLayer.Models.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceLayer.Models;

namespace DataAccessLayer
{
	public class ApplicationDbContext : IdentityDbContext<UserEntity, RoleEntity, Guid, 
		IdentityUserClaim<Guid>, UserRoleEntity, IdentityUserLogin<Guid>, 
		IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
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

			modelBuilder.Entity<IdentityUserLogin<Guid>>()
				.HasKey(l => new { l.LoginProvider, l.ProviderKey });

			modelBuilder.Entity<UserRoleEntity>()
				.HasKey(ur => new { ur.UserId, ur.RoleId });

			modelBuilder.Entity<IdentityUserToken<Guid>>()
				.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
		}

		public DbSet<IdentityUserClaim<Guid>> UserClaims { get; set; }
		public DbSet<IdentityUserLogin<Guid>> UserLogins { get; set; }
		public DbSet<IdentityRoleClaim<Guid>> RoleClaims { get; set; }
		public DbSet<IdentityUserToken<Guid>> UserTokens { get; set; }

		public DbSet<TelegramBannedUsersEntity> BanedUsers { get; set; } = null!;

		public DbSet<UserEntity> Users { get; set; }

		public DbSet<RoleEntity> Roles { get; set; }

		public DbSet<PermissionEntity> Permissions { get; set; }

		public DbSet<UserRoleEntity> UserRoleEntity { get; set; }

		public DbSet<LogEntity> Logs { get; set; }

		public DbSet<TelegramUserEntity> TelegramUsers { get; set; }

		public DbSet<TelegramPermissionsEntity> TelegramPermissions { get; set; }

		public DbSet<ExternalLoginEntity> ExternalLogins { get; set; }
	}
}
