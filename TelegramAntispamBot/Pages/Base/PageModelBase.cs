using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TelegramAntispamBot.Pages.Base
{
	public class PageModelBase : PageModel
	{
		public Guid UserId { get; set; }

		public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
		{
			var uId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

			if (!string.IsNullOrEmpty(uId))
			{
				UserId = Guid.Parse(uId);
			}

			base.OnPageHandlerExecuting(context);
		}
	}
}
