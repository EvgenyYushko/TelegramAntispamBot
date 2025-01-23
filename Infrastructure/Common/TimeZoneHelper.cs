using System;

namespace Infrastructure.Common
{
	public static class TimeZoneHelper
	{
		private static readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Europe/Minsk");

		public static DateTime DateTimeNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZoneInfo).ToUniversalTime();
	}
}
