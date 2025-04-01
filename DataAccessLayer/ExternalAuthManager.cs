using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainLayer.Models.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Infrastructure.Common.TimeZoneHelper;
using static Infrastructure.Helpers.AuthorizeHelper;
using static Infrastructure.Helpers.Logger;

namespace DataAccessLayer
{
	public class ExternalAuthManager
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<UserEntity> _userManager;

		public ExternalAuthManager(ApplicationDbContext dbContext, SignInManager<UserEntity> signInManager,
			UserManager<UserEntity> userManager)
		{
			_context = dbContext;
			_userManager = userManager;
		}

		public async Task<UserEntity> FindUserByEmail(string email)
		{
			var userEntity = await _context.Users
				.Include(u => u.Roles)
				.Include(u => u.ExternalLogins)
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Email == email);

			return userEntity;
		}

		public async Task<UserEntity> FindUserById(Guid id)
		{
			return await _userManager.FindByIdAsync(id.ToString());
		}

		public async Task<IdentityResult> RegisterExternalUserAsync(string username, string email, string role)
		{
			var temporaryPassword = GenerateTemporaryPassword();

			var user = new UserEntity { UserName = username, Email = email };
			var result = await _userManager.CreateAsync(user, temporaryPassword);
			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(user, role);
			}

			return result;
		}

		public async Task LinkExternalLoginAsync(UserEntity user, string provider, string providerKey)
		{
			var externalLogin = new ExternalLoginEntity
			{
				Id = Guid.NewGuid(),
				Provider = provider,
				ProviderKey = providerKey,
				UserId = user.Id,
				DateAdd = DateTimeNow
			};

			_context.ExternalLogins.Add(externalLogin);
			await _context.SaveChangesAsync();
		}

		public async Task<UserEntity> FindUserByExternalLoginAsync(string provider, string providerKey)
		{
			var userEntity = await _context.Users
				.Include(u => u.ExternalLogins)
				.FirstOrDefaultAsync(u =>
					u.ExternalLogins.Any(e => e.Provider == provider && e.ProviderKey == providerKey));

			var userEntityTest = _context.ExternalLogins
				.ToList();

			foreach (var login in userEntityTest)
			{
				Log($"login={login.Id}");
			}

			Log($"FindUserByExternalLoginAsync -> info.LoginProvider={provider}, info.ProviderKey={providerKey}");

			return userEntity;
		}

		public async Task<ExternalLoginEntity> ExistsExternalLoginAsync(Guid userId, string provider)
		{
			return await _context.ExternalLogins
				.FirstOrDefaultAsync(e => e.UserId == userId && e.Provider == provider);
		}

		public async Task<IList<ExternalLoginEntity>> GetExternalLoginsAsync(Guid userId)
		{
			return await _context.ExternalLogins
				.Where(e => e.UserId == userId)
				.ToListAsync();
		}

		public async Task RemoveExternalLoginAsync(Guid userId, Guid externalLoginId)
		{
			var externalLogin = await _context.ExternalLogins
				.FirstOrDefaultAsync(e => e.UserId == userId && e.Id == externalLoginId);

			if (externalLogin != null)
			{
				_context.ExternalLogins.Remove(externalLogin);
				await _context.SaveChangesAsync();
			}
		}
	}
}