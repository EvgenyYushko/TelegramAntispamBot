using Infrastructure.Models;

namespace Infrastructure.Helpers
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
