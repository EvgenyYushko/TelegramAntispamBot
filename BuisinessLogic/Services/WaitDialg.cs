using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BuisinessLogic.Services
{
	public class WaitDialg : IDisposable
	{
		private TelegramBotClient _telegramClient;
		private readonly long _userId;
		private CancellationTokenSource _cts;
		private Task _animationTask;
		private Message _msgProgress;

		public WaitDialg(TelegramBotClient telegramClient, long userId)
		{
			_telegramClient = telegramClient;
			_userId = userId;

			Console.WriteLine("WaitDialg Create");
		}

		public WaitDialg Show()
		{
			Task.Run(async () => await ShowWaitDialog());
			return this;
		}

		private async Task ShowWaitDialog()
		{
			Console.WriteLine("ShowWaitDialog Start");

			_cts = new CancellationTokenSource();

			// Отправляем начальное сообщение СНАЧАЛА
			_msgProgress = await _telegramClient.SendTextMessageAsync(
				chatId: _userId,
				text: "⏳" // Начальный символ
			);
			// Запускаем анимацию в ОТДЕЛЬНОМ ПОТОКЕ
			_animationTask = Task.Run(async () =>
			{
				bool showHourglass = true; // Флаг для отслеживания текущего символа

				try
				{
					while (!_cts.Token.IsCancellationRequested)
					{
						var currentSymbol = showHourglass ? "⌛" : "⏳";
						showHourglass = !showHourglass;

						// Обновляем уже отправленное сообщение
						await _telegramClient.EditMessageTextAsync(
							chatId: _userId,
							messageId: _msgProgress.MessageId,
							text: currentSymbol
						);

						await Task.Delay(1500, _cts.Token); // Задержка с учётом токена
					}
				}
				catch (OperationCanceledException)
				{
					Console.WriteLine("ShowWaitDialog OperationCanceledException");
					// Ожидаемое прерывание при отмене
				}
				catch (Exception ex)
				{
					// Логирование ошибок анимации
					Console.WriteLine($"Animation error: {ex.Message}");
				}
			});

			Console.WriteLine("ShowWaitDialog End");
		}

		public void Dispose()
		{
			Console.WriteLine("ShowWaitDialog.Dispose Start");

			// Останавливаем анимацию в любом случае
			_cts.Cancel();
			Task.Run(async () => await Close()).Wait();

			Console.WriteLine("ShowWaitDialog.Dispose End");
		}

		private async Task Close()
		{
			Console.WriteLine("ShowWaitDialog.Close Start");

			await _animationTask;

			await _telegramClient.DeleteMessageAsync(
				chatId: _userId,
				messageId: _msgProgress.MessageId
			);

			Console.WriteLine("ShowWaitDialog.Close End");
		}
	}
}
