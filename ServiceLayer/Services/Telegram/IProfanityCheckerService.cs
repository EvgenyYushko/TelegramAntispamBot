namespace ServiceLayer.Services.Telegram
{
	public interface IProfanityCheckerService
	{
		/// <summary>
		/// Проверить текс на наличие мат
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public bool ContainsProfanity(string text);
	}
}
