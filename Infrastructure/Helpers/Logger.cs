using System;

namespace Infrastructure.Helpers
{
	public static class Logger
	{
		public static void Log(string text)
		{
			Console.WriteLine(text);
		}
		
		public static void Log(object obj)
		{
			Console.WriteLine(obj);
		}
	}
}
