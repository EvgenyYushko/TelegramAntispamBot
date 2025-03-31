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
		private CancellationTokenSource _cts = new();
		private Message? _msgProgress;
		private bool _disposed;

		public WaitDialog(TelegramBotClient telegramClient, long userId)
		{
			_telegramClient = telegramClient;
			_userId = userId;
		}

		public WaitDialog Show()
		{
			_ = ShowWaitDialogAsync();
			return this;
		}

		private async Task ShowWaitDialogAsync()
		{
			try
			{
				await Task.Delay(600, _cts.Token);

				_msgProgress = await _telegramClient.SendTextMessageAsync(
					_userId,
					"⏳",
					cancellationToken: _cts.Token
				);

				_animationTask = AnimateMessageAsync();
			}
			catch (OperationCanceledException)
			{
				// Игнорируем, если отменили до отправки
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to show wait dialog: {ex.Message}");
			}
		}

		private async Task AnimateMessageAsync()
		{
			var showHourglass = true;
			while (!_cts.Token.IsCancellationRequested)
			{
				try
				{
					var symbol = showHourglass ? "⌛" : "⏳";
					showHourglass = !showHourglass;

					await _telegramClient.EditMessageTextAsync(
						_userId,
						_msgProgress!.MessageId,
						symbol,
						cancellationToken: _cts.Token
					);

					await Task.Delay(1500, _cts.Token);
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Animation error: {ex.Message}");
					break;
				}
			}
		}

		public async ValueTask DisposeAsync()
		{
			if (_disposed)
				return;
			_disposed = true;

			_cts.Cancel();

			try
			{
				await _animationTask;

				if (_msgProgress != null)
				{
					await _telegramClient.DeleteMessageAsync(
						_userId,
						_msgProgress.MessageId
					);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to clean up wait dialog: {ex.Message}");
			}
			finally
			{
				_cts.Dispose();
			}
		}

		public void Dispose() => DisposeAsync().GetAwaiter().GetResult();
	}
}