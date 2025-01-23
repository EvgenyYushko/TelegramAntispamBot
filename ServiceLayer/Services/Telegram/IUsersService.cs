using System;
using System.Threading.Tasks;
using ServiceLayer.Models;

namespace ServiceLayer.Services.Telegram
{
	public interface IUsersService
	{
		Task Register(string userName, string email, string password, string role);

		Task<string> Login(string email, string password);

		Task<UserAccount> GetUserById(Guid id);
	}
}
