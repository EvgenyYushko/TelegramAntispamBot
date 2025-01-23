using System;

namespace Infrastructure.Common
{
	public static class TimeZoneHelper
	{
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

		public static DateTime DateTimeNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZoneInfo).ToUniversalTime();
	}
}
