using System;
using System.Linq;
using System.Threading.Tasks;
using DomainLayer.Repositories;
using Infrastructure.Enumerations;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using ServiceLayer.Models;

namespace TelegramAntispamBot.Filters
{
	public class LogPageFilter : Attribute, IAsyncResourceFilter
	{
		private readonly IMemoryCache _cache;
		private readonly ILogRepository _logRepository;

		public LogPageFilter(ILogRepository logRepository, IMemoryCache cache)
		{
			_logRepository = logRepository;
			_cache = cache;
		}

		public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
		{
			var url = context.HttpContext.Request.GetDisplayUrl();
			var ip = GetRealIp(context.HttpContext.Request.Headers["X-Forwarded-For"].ToString());

			if (!_cache.TryGetValue(ip, out var value))
			{
				await _logRepository.Log(new Log
				{
					Type = LogType.Visit,
					Message = $"URL: {url}, IP: {ip}"
				});

				_cache.Set(ip, true, TimeSpan.FromMinutes(5));
			}

			await next();
		}

		private string GetRealIp(string headerXForwardedFor)
		{
			if (string.IsNullOrEmpty(headerXForwardedFor))
			{
				return "undefined";
			}

			return headerXForwardedFor.Split(',').FirstOrDefault().Trim();
		}
	}
}