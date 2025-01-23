using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramAntispamBot.DomainLayer.Models
{
	public class JwtOptions
	{
		public string SecretKey { get; set; } = string.Empty;
		public int ExpiresHours { get; set; }
	}
}
