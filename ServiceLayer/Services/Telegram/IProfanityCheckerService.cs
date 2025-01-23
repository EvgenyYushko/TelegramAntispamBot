namespace ServiceLayer.Services.Telegram
{
	public interface IProfanityCheckerService
	{
		public bool ContainsProfanity(string text);
	}
}
