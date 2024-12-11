using System;
using System.IO;
using System.Linq;
using TelegramAntispamBot.DomainLayer.Repositories;

namespace TelegramAntispamBot.DataAccessLayer.Repositories
{
	public class ProfanityCheckerRepository : IProfanityCheckerRepository
	{
		private readonly string[] _bannedWords;
		private string FILE_PATH = Path.Combine(AppContext.BaseDirectory, "Resources", "mat.txt");

		public ProfanityCheckerRepository()
		{
			// Загружаем слова из файла в массив
			if (!File.Exists(FILE_PATH))
			{
				throw new FileNotFoundException($"File {FILE_PATH} not Found.");
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
