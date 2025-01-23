using System;
using Infrastructure.Enumerations;

namespace ServiceLayer.Models
{
	public class Log
	{
		public DateTime DateTime { get; set; }
		public LogType Type { get; set; }
		public string Message { get; set; }
	}
}
