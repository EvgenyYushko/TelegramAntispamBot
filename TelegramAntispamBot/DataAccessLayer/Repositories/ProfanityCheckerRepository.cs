using System.IO;
using System.Linq;
using TelegramAntispamBot.DomainLayer.Repositories;

namespace TelegramAntispamBot.DataAccessLayer.Repositories
{
	public class ProfanityCheckerRepository : IProfanityCheckerRepository
	{
		private readonly string[] _bannedWords;
		private const string FILE_PATH = "Resources/mat.txt";

		public ProfanityCheckerRepository()
		{
			// Загружаем слова из файла в массив
			if (!File.Exists(FILE_PATH))
			{
				throw new FileNotFoundException($"Файл {FILE_PATH} не найден.");
			}

			_bannedWords = File.ReadAllLines(FILE_PATH)
				.Select(word => word.Trim()) // Убираем пробелы
				.Where(word => !string.IsNullOrWhiteSpace(word)) // Исключаем пустые строки
				.ToArray();
		}

		public string[] GetProfanityData()
		{
			return _bannedWords;
		}
	}
}
