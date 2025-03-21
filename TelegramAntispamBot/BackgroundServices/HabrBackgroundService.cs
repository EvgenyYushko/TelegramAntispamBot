﻿using System;
using System.Threading.Tasks;
using BuisinessLogic.Services.Parsers;
using Infrastructure.InjectSettings;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.BackgroundServices.Base;

namespace TelegramAntispamBot.BackgroundServices
{
	public class HabrBackgroundService : BaseSiteBackgroundService
	{
		private readonly HabrParser _habrParser;

		public HabrBackgroundService(TelegramInject botClient, HabrParser habrParser, ITelegramUserService telegramUserService)
			: base(botClient, new BackgroundSiteSetting
			{
				ScheduledTimes = new[]
				{
					new TimeSpan(11, 0, 0), // 11:00
					//new TimeSpan(16, 0, 0)  // 16:00
				}
			},
			telegramUserService)
		{
			_habrParser = habrParser;
		}

		/// <inheritdoc />
		protected override Task<string> Parse()
		{
			return _habrParser.ParseLatestPostAsync();
		}
	}
}
