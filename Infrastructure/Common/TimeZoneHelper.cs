using System;

namespace Infrastructure.Common
{
	public static class TimeZoneHelper
	{
		public static DateTime DateTimeNow =>
			TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZoneInfo).ToUniversalTime();

		private static TimeZoneInfo _timeZoneInfo
		{
			get
			{
				try
				{
					return TimeZoneInfo.FindSystemTimeZoneById("Europe/Minsk");
				}
				catch
				{
					return TimeZoneInfo.Local;
				}
			}
		}
	}
}