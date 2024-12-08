namespace TelegramAntispamBot.DomainLayer.Repositories
{
	public interface IProfanityCheckerRepository
	{
		public string[] GetProfanityData();
	}
}
