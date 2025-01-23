using ServiceLayer.Models;

namespace ServiceLayer.Services.Authorization
{
	public interface IJwtProvider
	{
		string GenerateToken(UserAccount user);
	}
}
