using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainLayer.Models.Authorization;
using DomainLayer.Repositories;
using Infrastructure.Enumerations;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Models;

namespace DataAccessLayer.Repositories
{
	public class UsersAccountRepository : IUsersAccountRepository
	{
		private readonly ApplicationDbContext _context;

		public UsersAccountRepository(ApplicationDbContext dbContext)
		{
			_context = dbContext;
		}

		public async Task Add(UserAccount user)
		{
			var roleId = (int)user.Roles.First();

			var roleEntity = await _context.Roles
				.SingleOrDefaultAsync(r => r.Id == roleId);
			

			var userEntity = new UserEntity()
			{
				Id = user.Id,
				UserName = user.UserName,
				PasswordHash = user.PasswordHash,
				Email = user.Email,
				Roles = new List<RoleEntity>(){ roleEntity}
			};

			await _context.Users.AddAsync(userEntity);
			await _context.SaveChangesAsync();
		}

		public async Task<UserAccount> GetByEmail(string email)
		{
			var userEntity = await _context.Users
					.Include(u => u.Roles)
					.AsNoTracking()
					.FirstOrDefaultAsync(u => u.Email == email);

			if (userEntity is null)
			{
				return null;
			}

			var user = new UserAccount(userEntity.Id,
				userEntity.UserName,
				userEntity.PasswordHash,
				userEntity.Email,
				userEntity.Roles.Select(r => (Role)r.Id).ToList());

			return user;
		}

		public async Task<UserAccount> GetByUserName(string userName)
		{
			var userEntity = await _context.Users
					.Include(u => u.Roles)
					.AsNoTracking()
					.FirstOrDefaultAsync(u => u.UserName == userName);

			if (userEntity is null)
			{
				return null;
			}

			var user = new UserAccount(userEntity.Id,
				userEntity.UserName,
				userEntity.PasswordHash,
				userEntity.Email,
				userEntity.Roles.Select(r => (Role)r.Id).ToList());

			return user;
		}

		public async Task<UserAccount> GetById(Guid id)
		{
			var userEntity = await _context.Users
				.Include(u => u.Roles)
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == id) ?? throw new Exception();

			var user = new UserAccount(userEntity.Id,
				userEntity.UserName,
				userEntity.PasswordHash,
				userEntity.Email,
				userEntity.Roles.Select(r => (Role)r.Id).ToList());

			return user;
		}

		public async Task<HashSet<Permission>> GetUserPermissions(Guid userId)
		{
			var roles = await _context.Users
				.AsNoTracking()
				.Include(u => u.Roles)
				.ThenInclude(r => r.Permissions)
				.Where(u => u.Id == userId)
				.Select(u => u.Roles)
				.ToArrayAsync();

			return roles
				.SelectMany(r => r)
				.SelectMany(r => r.Permissions)
				.Select(p => (Permission)p.Id)
				.ToHashSet();
		}
	}
}
