using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ServiceLayer.Services.Telegram
{
	public interface IDeleteMessageService
	{
		public Task DeleteMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string msg, InlineKeyboardMarkup inlineKeyboard =null);
	}
}
