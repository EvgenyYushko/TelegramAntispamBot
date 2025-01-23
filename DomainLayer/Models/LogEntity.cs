using System;
using Infrastructure.Enumerations;

namespace DomainLayer.Models
{
	public class LogEntity
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public LogType Type { get; set; }
		public DateTime DateTime { get; set; }
		public string Message { get; set; }
	}
}
