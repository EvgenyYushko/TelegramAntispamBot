using System;
using System.Collections.Generic;

namespace TelegramAntispamBot.Jobs.Helpers
{
	public static class JobHelper
	{
		public static string HabrKey ="Habr";
		public static string OnlinerKey ="Onliner";
		public static string CurrencyKey ="Currency";
		public static string TrainModelKey ="TrainMode";
		public static string SendMailKey ="SendMail";

		public static string ChatId ="chatId";
		public static List<JobsSetting> JobSettings { get; set; } = new();

		static JobHelper()
		{
			JobSettings.Add(new () { Type = typeof(HabrJob), Key = HabrKey, Time = "0 0 11 * * ?", Castum = true });
			JobSettings.Add(new () { Type = typeof(OnlinerJob), Key = OnlinerKey, Time = "0 0 13 * * ?", Castum = true });
			JobSettings.Add(new () { Type = typeof(CurrencyJob), Key = CurrencyKey, Time = "0 0 9 * * ?", Castum = true });
			JobSettings.Add(new () { Type = typeof(TrainModeJob), Key = TrainModelKey, Time = "0 0 23 * * ?", Castum = true });
			JobSettings.Add(new () { Type = typeof(SendMailJob), Key = SendMailKey, Time = "0 0 10 ? * MON", Castum = true });
		}
	}

	public class JobsSetting
	{
		public Type Type { get; set; }
		public string Key { get; set; }
		public string Time { get; set; }
		public bool Castum { get; set; }
	}
}
