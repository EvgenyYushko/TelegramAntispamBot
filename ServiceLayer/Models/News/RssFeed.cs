namespace ServiceLayer.Models.News
{
	public class RssFeed
	{
		public string Name { get; set; }
		public string Url { get; set; }
		public string Description { get; set; }
		public string Category { get; set; }
		public bool RequiresVpn { get; set; }
	}
}
