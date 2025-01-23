using TelegramAntispamBot.DomainLayer.Models;

namespace TelegramAntispamBot.ServiceLayer.Authorization
{
	public interface IJwtProvider
	{
		string GenerateToken(UserAccount user);

	}
}
