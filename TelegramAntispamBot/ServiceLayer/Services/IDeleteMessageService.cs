using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramAntispamBot.ServiceLayer.Services
{
	public interface IDeleteMessageService
	{
		public Task DeleteMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string msg);
	}
}
