using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainLayer.Repositories;
using Infrastructure.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly ApplicationDbContext _context;

		public UserRepository(ApplicationDbContext dbContext)
		{
			_context = dbContext;
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
