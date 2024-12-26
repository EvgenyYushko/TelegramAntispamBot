using TelegramAntispamBot.DomainLayer.Models;

namespace TelegramAntispamBot.Helpers
{
	public static class UserHelper
	{
		public static void ResetPull(this UserInfo user)
		{
			user.PullModel.PollMessageId = 0;
			user.PullModel.Message = null;
			user.PullModel.PullId = null;
		}
	}
}
