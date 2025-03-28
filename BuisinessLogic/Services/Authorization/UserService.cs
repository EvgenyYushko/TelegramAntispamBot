using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainLayer.Models.Authorization;
using Infrastructure.Enumerations;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Services.Authorization;

namespace BuisinessLogic.Services.Authorization
{
	public class UserService : IUserService
	{
		private readonly SignInManager<UserEntity> _signInManager;
		private readonly UserManager<UserEntity> _userManager;

		public UserService(SignInManager<UserEntity> signInManager
			, UserManager<UserEntity> userManager)
		{
			_signInManager = signInManager;
			_userManager = userManager;
		}

		public async Task<IdentityResult> Register(string userName, string email, string password, string role)
		{
			var user = new UserEntity { UserName = userName, Email = email };
			var result = await _userManager.CreateAsync(user, password);
			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(user, role);
				await _signInManager.SignInAsync(user, false);
			}

			return result;
		}

		public async Task<IdentityResult> UpdateRole(Guid id, Role role)
		{
			var user = await _userManager.FindByIdAsync(id.ToString());
			if (user is null)
			{
				throw new Exception($"Пользователь не найдент id = {id}");
			}

			var roles = await _userManager.GetRolesAsync(user);
			var result = await _userManager.RemoveFromRolesAsync(user, roles);

			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(user, role.ToString());
			}

			return result;
		}

		public async Task<SignInResult> Login(string userName, string password)
		{
			return await _signInManager.PasswordSignInAsync(userName,password,false,false);
		}

		public async Task<IdentityResult> Delete(Guid id)
		{
			var user = await _userManager.FindByIdAsync(id.ToString());
			if (user != null)
			{
				return await _userManager.DeleteAsync(user);
			}

			return null;
		}

		public async Task<UserAccount> GetUserById(Guid id)
		{
			var userEntity = await _userManager.FindByIdAsync(id.ToString());
			if (userEntity is null)
			{
				throw new Exception($"Пользователь не найдент id = {id}");
			}

			var roles = await _userManager.GetRolesAsync(userEntity);

			var user = new UserAccount(userEntity.Id,
				userEntity.UserName,
				userEntity.PasswordHash,
				userEntity.Email,
				roles.Select(r => Enum.Parse<Role>(r)).ToList());

			return user;
		}

		public List<UserAccount> GetAllUsers()
		{
			var userEntity = _userManager.Users.Include(u => u.Roles);
			if (userEntity is null)
			{
				throw new Exception("Пользователей не найдено");
			}

			return userEntity.Select(u =>
				new UserAccount(u.Id,
					u.UserName,
					u.PasswordHash,
					u.Email,
					u.Roles.Select(r => Enum.Parse<Role>(r.Name)).ToList())
			).ToList();
		}
	}
}