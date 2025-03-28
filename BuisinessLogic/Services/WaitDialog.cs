using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BuisinessLogic.Services
{
	public class WaitDialog : IDisposable
	{
		private readonly TelegramBotClient _telegramClient;
		private readonly long _userId;
		private Task _animationTask = Task.CompletedTask;
		private CancellationTokenSource _cts;
		private bool _messageSent;
		private Message _msgProgress;

		public WaitDialog(TelegramBotClient telegramClient, long userId)
		{
			_telegramClient = telegramClient;
			_userId = userId;
		}

		public WaitDialog Show()
		{
			// Запускаем асинхронно, но не блокируем поток
			_ = ShowWaitDialogAsync();
			return this;
		}

		private async Task ShowWaitDialogAsync()
		{
			_cts = new CancellationTokenSource();

			try
			{
				// Ждем 500 мс перед отправкой сообщения
				await Task.Delay(500, _cts.Token);

				// Если отмены не было, отправляем сообщение
				_msgProgress = await _telegramClient.SendTextMessageAsync(_userId, "⏳");
				_messageSent = true;

				// Запускаем анимацию
				_animationTask = Task.Run(async () =>
				{
					var showHourglass = true;
					try
					{
						while (!_cts.Token.IsCancellationRequested)
						{
							var symbol = showHourglass ? "⌛" : "⏳";
							showHourglass = !showHourglass;

							await _telegramClient.EditMessageTextAsync(
								_userId,
								_msgProgress.MessageId,
								symbol
							);

							await Task.Delay(1500, _cts.Token);
						}
					}
					catch (OperationCanceledException)
					{
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Animation error: {ex.Message}");
					}
				});
			}
			catch (OperationCanceledException)
			{
				// Отмена до отправки сообщения — игнорируем
			}
		}

		public void Dispose()
		{
			_cts?.Cancel();
			CloseAsync().Wait();
		}

		private async Task CloseAsync()
		{
			try
			{
				await _animationTask;
			}
			catch (OperationCanceledException)
			{
			}

			// Удаляем сообщение только если оно было отправлено
			if (_messageSent && _msgProgress != null)
			{
				await _telegramClient.DeleteMessageAsync(_userId, _msgProgress.MessageId);
			}
		}
	}
}