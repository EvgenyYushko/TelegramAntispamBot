using System;
using System.Threading.Tasks;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.Jobs.Base;
using static Infrastructure.Helpers.Logger;
using static TelegramAntispamBot.Jobs.Helpers.JobHelper;

namespace TelegramAntispamBot.Pages
{
	[Authorize]
	public class ChatProfileModel : PageModel
	{
		private long _chatId;
		private Chanel _chat;
		private ITelegramUserService _telegramUserService;
		private readonly ScheduleInspectorService _scheduleInspectorService;

		public ChatProfileModel(ITelegramUserService telegramUserService, ScheduleInspectorService scheduleInspectorService)
		{
			_telegramUserService = telegramUserService;
			_scheduleInspectorService = scheduleInspectorService;
		}

		[BindProperty]
		public Chanel Chat => _chat ??= _telegramUserService.GetChatById(_chatId);

		public async Task<IActionResult> OnGetAsync(long chatId)
		{
			if (User.Identity.IsAuthenticated)
			{
				_chatId = chatId;
				Log(Chat);
				return Page();
			}

			return RedirectToPage("/Account/Register");
		}

		public async Task OnGetSetAllowSendNews(bool isAllowed, long chatId)
		{
			_chatId = chatId;

			await _telegramUserService.UpdateChatPermissions(new ChatPermissions()
			{
				ChatId = chatId,
				SendNews = isAllowed
			});
		}

		public void ValidateCronExpression(string cronExpression)
		{
			if (!_scheduleInspectorService.ValidateCronExpression(cronExpression))
			{
				throw new Exception("Expression invalid");
			}
		}

		public async Task OnGetSetAllowCurrency(bool isAllowed, string cronExpression, long chatId)
		{
			_chatId = chatId;

			if (isAllowed)
			{
				ValidateCronExpression(cronExpression);
				await _scheduleInspectorService.UpdateChatScheduleAsync(chatId, CurrencyKey, cronExpression);
			}
			else
			{
				await _scheduleInspectorService.RemoveChatTrigger(CurrencyKey, chatId);
			}

			var chat = _telegramUserService.GetChatById(chatId);
			chat.ChatPermission.SendCurrency = isAllowed;
			chat.ChatPermission.CurrencyCronExpression = cronExpression;

			await _telegramUserService.UpdateChatPermissions(chat.ChatPermission);
		}

		public async Task OnGetSetAllowHabrNews(bool isAllowed, string cronExpression, long chatId)
		{
			_chatId = chatId;

			if (isAllowed)
			{
				ValidateCronExpression(cronExpression);
				await _scheduleInspectorService.UpdateChatScheduleAsync(chatId, HabrKey, cronExpression);
			}
			else
			{
				await _scheduleInspectorService.RemoveChatTrigger(HabrKey, chatId);
			}

			var chat = _telegramUserService.GetChatById(chatId);
			chat.ChatPermission.SendHabr = isAllowed;
			chat.ChatPermission.HabrCronExpression = cronExpression;

			await _telegramUserService.UpdateChatPermissions(chat.ChatPermission);
		}

		public async Task OnGetSetAllowOnlinerNews(bool isAllowed, string cronExpression, long chatId)
		{
			_chatId = chatId;

			if (isAllowed)
			{
				ValidateCronExpression(cronExpression);
				await _scheduleInspectorService.UpdateChatScheduleAsync(chatId, OnlinerKey, cronExpression);
			}
			else
			{
				await _scheduleInspectorService.RemoveChatTrigger(OnlinerKey, chatId);
			}

			var chat = _telegramUserService.GetChatById(chatId);
			chat.ChatPermission.SendOnliner = isAllowed;
			chat.ChatPermission.OnlinerCronExpression = cronExpression;

			await _telegramUserService.UpdateChatPermissions(chat.ChatPermission);
		}
	}
}