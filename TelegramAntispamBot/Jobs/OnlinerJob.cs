﻿using System.Threading.Tasks;
using BuisinessLogic.Services.Parsers;
using Infrastructure.InjectSettings;
using Infrastructure.Models.AI;
using ServiceLayer.Services.Telegram;
using TelegramAntispamBot.Jobs.Base;

namespace TelegramAntispamBot.Jobs
{
	public class OnlinerJob : SchedulerJob
	{
		private readonly OnlinerParser _onlinerParser;

		public OnlinerJob(TelegramInject botClient, OnlinerParser onlinerParser, ITelegramUserService telegramUserService)
			: base(botClient, telegramUserService)
		{
			_onlinerParser = onlinerParser;
		}

		/// <inheritdoc />
		protected override Task<string> Parse(ParseParams parseParams)
		{
			return _onlinerParser.ParseLatestPostAsync();
		}
	}
}