using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramAntispamBot.ServiceLayer.Services
{
	public interface IProfanityCheckerService
	{
		public bool ContainsProfanity(string text);
	}
}
