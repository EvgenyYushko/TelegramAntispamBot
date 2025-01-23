using System;
using System.Threading.Tasks;
using TelegramAntispamBot.DomainLayer.Models;

namespace TelegramAntispamBot.ServiceLayer.Services
{
	public interface IUsersService
	{
		Task Register(string userName, string email, string password, string role);

		Task<string> Login(string email, string password);

		Task<UserAccount> GetUserById(Guid id);
	}
}
