﻿using System;
using System.Threading.Tasks;
using Infrastructure.Models;

namespace ServiceLayer.Services.Authorization
{
	public interface IUserService
	{
		Task Register(string userName, string email, string password, string role);

		Task<string> Login(string email, string password);

		Task<UserAccount> GetUserById(Guid id);

		Task<UserAccount> GetUserByName(string userName);

		Task<UserAccount> GetByEmail(string eMail);
	}
}
