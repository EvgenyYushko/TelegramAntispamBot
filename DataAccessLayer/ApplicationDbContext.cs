using System;
using System.Threading.Channels;
using DataAccessLayer.Configurations;
using DomainLayer.Models;
using DomainLayer.Models.Authorization;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceLayer.Models;
using Telegram.Bot.Types;

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

			// Конфигурация для ChannelAdmin
			modelBuilder.Entity<TelegramChannelAdmin>()
				.HasKey(ca => new { ca.UserId, ca.ChannelId });

			// Конфигурация для UserChannelMembership
			modelBuilder.Entity<UserChannelMembership>()
				.HasKey(ucm => new { ucm.UserId, ucm.ChannelId });

			modelBuilder.Entity<TelegramChannel>()
				.HasKey(ucm => new { ucm.Id });

			// Связи для членства
			modelBuilder.Entity<UserChannelMembership>()
				.HasOne(ucm => ucm.User)
				.WithMany(u => u.ChannelMemberships)
				.HasForeignKey(ucm => ucm.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<UserChannelMembership>()
				.HasOne(ucm => ucm.Channel)
				.WithMany(c => c.Members)
				.HasForeignKey(ucm => ucm.ChannelId)
				.OnDelete(DeleteBehavior.Cascade);

			// Конфигурация для создателя канала
			modelBuilder.Entity<TelegramChannel>()
				.HasOne(c => c.Creator)
				.WithMany(u => u.OwnedChannels)
				.HasForeignKey(c => c.CreatorId)
				.OnDelete(DeleteBehavior.Restrict);

			// Конфигурация связей администраторов
			modelBuilder.Entity<TelegramChannelAdmin>()
				.HasOne(ca => ca.User)
				.WithMany(u => u.AdminInChannels)
				.HasForeignKey(ca => ca.UserId)
				.OnDelete(DeleteBehavior.Cascade)				;

			modelBuilder.Entity<TelegramChannelAdmin>()
				.HasOne(ca => ca.Channel)
				.WithMany(c => c.Admins)
				.HasForeignKey(ca => ca.ChannelId)
				.OnDelete(DeleteBehavior.Cascade);

			// настройка индексов
			modelBuilder.Entity<TelegramUserEntity>()
				.HasIndex(u => u.UserId)
				.IsUnique();

			modelBuilder.Entity<TelegramUserEntity>()
				.HasIndex(c => c.UserId)
				.IsUnique();
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

		public DbSet<ChatPermissionsEntity> ChatPermissions { get; set; }

		public DbSet<ExternalLoginEntity> ExternalLogins { get; set; }

		public DbSet<TelegramChannel> TelegramChanel { get; set; }

		public DbSet<TelegramChannelAdmin> TelegramChannelAdmin { get; set; }

		public DbSet<UserChannelMembership> UserChannelMembership { get; set; }

		public DbSet<SuspiciousMessage> SuspiciousMessages { get; set; } // Таблица для хранения сообщений
		public DbSet<ChatMessagesEntity> ChatMessages { get; set; }
	}
}
