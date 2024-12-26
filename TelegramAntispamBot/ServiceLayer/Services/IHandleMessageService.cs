using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAntispamBot.InjectSettings;

namespace TelegramAntispamBot.ServiceLayer.Services
{
	public interface IHandleMessageService
	{
		public Task HandleUpdateAsync(TelegramInject botClient, Update update, UpdateType type, CancellationToken cancellationToken);
	}
}
