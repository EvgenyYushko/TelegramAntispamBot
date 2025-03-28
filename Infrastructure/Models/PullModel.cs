using System.Threading;
using Telegram.Bot.Types;

namespace Infrastructure.Models
{
	public class PullModel
	{
		public string PullId { get; set; }

		public int PollMessageId { get; set; }

		public Timer PullTimer { get; set; }

		public Message Message { get; set; }

		public int CountFoul { get; set; }
	}
}