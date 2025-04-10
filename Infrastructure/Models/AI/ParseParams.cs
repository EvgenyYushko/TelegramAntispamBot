using System.Collections.Generic;

namespace Infrastructure.Models.AI
{
	public class ParseParams
	{
		public string ChatTitle { get; set; }
		public string ChatDescription { get; set; }

		public List<string> lastMessages { get; set; }
	}
}
