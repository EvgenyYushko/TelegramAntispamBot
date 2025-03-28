using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TelegramAntispamBot.TagHelpers
{
	[HtmlTargetElement(Attributes = "policy")]
	public class PolicyTagHelper : TagHelper
	{
		private readonly IAuthorizationService _authorizationService;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public PolicyTagHelper(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
		{
			_authorizationService = authorizationService;
			_httpContextAccessor = httpContextAccessor;
		}

		public string Policy { get; set; }

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			await base.ProcessAsync(context, output);

			var hasNeedRole = (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Policy)).Succeeded;
			if (!hasNeedRole)
			{
				output.SuppressOutput();
			}
		}
	}
}