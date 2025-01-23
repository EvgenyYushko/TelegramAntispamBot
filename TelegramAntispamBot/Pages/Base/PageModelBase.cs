using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TelegramAntispamBot.Pages.Base
{
	public class PageModelBase : PageModel
	{
		public Guid UserId { get; set; }

		[TempData]
		public string ErrorMessage { get; set; } = string.Empty;

		public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
		{
			var token = context.HttpContext.Request.Cookies["token"];

			if (!string.IsNullOrEmpty(token))
			{
				var handler = new JwtSecurityTokenHandler();
				var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

				if (jsonToken != null)
				{
					UserId = Guid.Parse(jsonToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
				}
			}

			base.OnPageHandlerExecuting(context);
		}
	}
}
