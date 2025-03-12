using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Enumerations;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

namespace ServiceLayer.Services.Authorization
{
	public interface IUserService
	{
		Task<IdentityResult> Register(string userName, string email, string password, string role);

		Task<SignInResult> Login(string userName, string password);

		Task<IdentityResult> UpdateRole(Guid id, Role role);

		Task<IdentityResult> Delete(Guid id);

		Task<UserAccount> GetUserById(Guid id);

		List<UserAccount> GetAllUsers();
	}
}
