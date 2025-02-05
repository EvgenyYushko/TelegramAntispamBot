using System;
using System.Threading.Tasks;
using DomainLayer.Repositories;
using Infrastructure.Models;
using ServiceLayer.Services.Authorization;

namespace BuisinessLogic.Services.Authorization
{
	public class UserService : IUserService
	{
		private readonly IPasswordHasher _passwordHasher;
		private readonly IUserRepository _usersRepository;
		private readonly IJwtProvider _jwtProvider;

		public UserService(IUserRepository usersRepository, IPasswordHasher passwordHasher, IJwtProvider jwtProvider)
		{
			_usersRepository = usersRepository;
			_passwordHasher = passwordHasher;
			_jwtProvider = jwtProvider;
		}

		public async Task Register(string userName, string email, string password, string role)
		{
			var userByUserName = await _usersRepository.GetByUserName(userName);

			if (userByUserName != null)
			{
				throw new ArgumentException("Пользователь с данным логином уже существует.");
			}

			var userByEmail = await _usersRepository.GetByEmail(email);

			if (userByEmail != null)
			{
				throw new ArgumentException("Данный email уже зарегистрирован.");
			}

			var hashedPassword = _passwordHasher.Generate(password);

			var user = UserAccount.Create(userName, hashedPassword, email, role);

			await _usersRepository.Add(user);
		}

		public async Task<string> Login(string email, string password)
		{
			var user = await _usersRepository.GetByEmail(email);

			if (user is null)
			{
				throw new ArgumentException("Пользователь с данным email не зарегистрирован.");
			}

			var result = _passwordHasher.Verify(password, user.PasswordHash);

			if (result == false)
			{
				throw new Exception("Invalid password");
			}

			var token = _jwtProvider.GenerateToken(user);

			return token;
		}

		public async Task<UserAccount> GetUserById(Guid id)
		{
			var user = await _usersRepository.GetById(id);
			return user;
		}

		public async Task<UserAccount> GetUserByName(string userName)
		{
			return await _usersRepository.GetByUserName(userName);
		}

		public async Task<UserAccount> GetByEmail(string eMail)
		{
			return await _usersRepository.GetByEmail(eMail);
		}
	}
}
