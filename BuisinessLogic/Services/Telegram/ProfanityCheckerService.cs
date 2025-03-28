using System;
using System.Linq;
using DomainLayer.Repositories;
using ServiceLayer.Services.Telegram;

namespace BuisinessLogic.Services.Telegram
{
	public class ProfanityCheckerService : IProfanityCheckerService
	{
		private readonly IProfanityCheckerRepository _profanityCheckerRepository;

		public ProfanityCheckerService(IProfanityCheckerRepository profanityCheckerRepository)
		{
			_profanityCheckerRepository = profanityCheckerRepository;
		}

		/// <inheritdoc />
		public bool ContainsProfanity(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}

			// Проверяем каждое слово на совпадение
			return _profanityCheckerRepository.GetProfanityData().Any(bannedWord =>
				text.IndexOf(bannedWord, StringComparison.OrdinalIgnoreCase) >= 0);
		}
	}
}