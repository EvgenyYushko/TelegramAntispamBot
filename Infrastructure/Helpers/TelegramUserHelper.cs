using Infrastructure.Models;

namespace Infrastructure.Helpers
{
	public static class TelegramUserHelper
	{
		public static void ResetPull(this TelegramUser user)
		{
			user.PullModel.PollMessageId = 0;
			user.PullModel.Message = null;
			user.PullModel.PullId = null;
		}
	}
}
