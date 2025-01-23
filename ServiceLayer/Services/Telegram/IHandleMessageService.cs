using System.Threading;
using System.Threading.Tasks;
using Infrastructure.InjectSettings;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ServiceLayer.Services.Telegram
{
	public interface IHandleMessageService
	{
		public Task HandleUpdateAsync(TelegramInject botClient, Update update, UpdateType type, CancellationToken cancellationToken);
	}
}
