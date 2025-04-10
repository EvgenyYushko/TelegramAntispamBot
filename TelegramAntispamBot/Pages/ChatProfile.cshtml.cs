using System;
using System.Threading.Tasks;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLayer.Services.AI;
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
		private readonly IValidationErrorServiceAI _validationErrorServiceAI;

		public ChatProfileModel(ITelegramUserService telegramUserService, ScheduleInspectorService scheduleInspectorService
			, IValidationErrorServiceAI validationErrorServiceAI
			)
		{
			_telegramUserService = telegramUserService;
			_scheduleInspectorService = scheduleInspectorService;
			_validationErrorServiceAI = validationErrorServiceAI;
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

		public bool ValidateCronExpression(string cronExpression)
		{
			return _scheduleInspectorService.ValidateCronExpression(cronExpression);
		}

		public async Task<IActionResult> OnGetSetAllowAllNews(bool isAllowed, string cronExpression, long chatId)
		{
			_chatId = chatId;

			if (isAllowed)
			{
				if (!ValidateCronExpression(cronExpression))
				{
					var explaidText = await _validationErrorServiceAI.ExplainInvalidCronExpression(cronExpression);
					return BadRequest($"{(explaidText is null ? "Не корректное cron выражение" : $"{explaidText}")}");
				}

				await _scheduleInspectorService.UpdateChatScheduleAsync(chatId, AllNewsKey, cronExpression);
			}
			else
			{
				await _scheduleInspectorService.RemoveChatTrigger(AllNewsKey, chatId);
			}
			
			await _scheduleInspectorService.PrintScheduleInfo();

			var chat = _telegramUserService.GetChatById(chatId);
			chat.ChatPermission.SendNews = isAllowed;
			chat.ChatPermission.AllNewsCronExpression = cronExpression;

			await _telegramUserService.UpdateChatPermissions(chat.ChatPermission);

			return new JsonResult(new { success = true }); // для успешного ответа
		}

		public async Task<IActionResult> OnGetSetAllowCurrency(bool isAllowed, string cronExpression, long chatId)
		{
			_chatId = chatId;

			if (isAllowed)
			{
				if (!ValidateCronExpression(cronExpression))
				{
					var explaidText = await _validationErrorServiceAI.ExplainInvalidCronExpression(cronExpression);
					return BadRequest($"{(explaidText is null ? "Не корректное cron выражение" : $"{explaidText}")}");
				}

				await _scheduleInspectorService.UpdateChatScheduleAsync(chatId, CurrencyKey, cronExpression);
			}
			else
			{
				await _scheduleInspectorService.RemoveChatTrigger(CurrencyKey, chatId);
			}
			
			await _scheduleInspectorService.PrintScheduleInfo();

			var chat = _telegramUserService.GetChatById(chatId);
			chat.ChatPermission.SendCurrency = isAllowed;
			chat.ChatPermission.CurrencyCronExpression = cronExpression;

			await _telegramUserService.UpdateChatPermissions(chat.ChatPermission);

			return new JsonResult(new { success = true }); // для успешного ответа
		}

		public async Task<IActionResult> OnGetSetAllowHabrNews(bool isAllowed, string cronExpression, long chatId)
		{
			_chatId = chatId;

			if (isAllowed)
			{
				if (!ValidateCronExpression(cronExpression))
				{
					var explaidText = await _validationErrorServiceAI.ExplainInvalidCronExpression(cronExpression);
					return BadRequest($"{(explaidText is null ? "Не корректное cron выражение" : $"{explaidText}")}");
				}

				await _scheduleInspectorService.UpdateChatScheduleAsync(chatId, HabrKey, cronExpression);
			}
			else
			{
				await _scheduleInspectorService.RemoveChatTrigger(HabrKey, chatId);
			}

			await _scheduleInspectorService.PrintScheduleInfo();

			var chat = _telegramUserService.GetChatById(chatId);
			chat.ChatPermission.SendHabr = isAllowed;
			chat.ChatPermission.HabrCronExpression = cronExpression;

			await _telegramUserService.UpdateChatPermissions(chat.ChatPermission);

			return new JsonResult(new { success = true }); // для успешного ответа
		}

		public async Task<IActionResult> OnGetSetAllowOnlinerNews(bool isAllowed, string cronExpression, long chatId)
		{
			_chatId = chatId;

			if (isAllowed)
			{
				if (!ValidateCronExpression(cronExpression))
				{
					var explaidText = await _validationErrorServiceAI.ExplainInvalidCronExpression(cronExpression);
					return BadRequest($"{(explaidText is null ? "Не корректное cron выражение" : $"{explaidText}")}");
				}

				await _scheduleInspectorService.UpdateChatScheduleAsync(chatId, OnlinerKey, cronExpression);
			}
			else
			{
				await _scheduleInspectorService.RemoveChatTrigger(OnlinerKey, chatId);
			}

			await _scheduleInspectorService.PrintScheduleInfo();

			var chat = _telegramUserService.GetChatById(chatId);
			chat.ChatPermission.SendOnliner = isAllowed;
			chat.ChatPermission.OnlinerCronExpression = cronExpression;

			await _telegramUserService.UpdateChatPermissions(chat.ChatPermission);

			return new JsonResult(new { success = true }); // для успешного ответа
		}
	}
}