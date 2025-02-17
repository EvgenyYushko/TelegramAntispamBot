using Microsoft.Extensions.Primitives;
using Telegram.Bot.Types;

namespace TelegramAntispamBot
{
	internal class UserTeleg : User
	{
		public long Id { get; set; }
		public StringValues FirstName { get; set; }
		public StringValues LastName { get; set; }
		public StringValues Username { get; set; }
		public StringValues Hash { get; set; }
		public long AuthDate { get; set; }
	}
}