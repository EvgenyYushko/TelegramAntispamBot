using System;
using System.Threading.Tasks;
using Infrastructure.Common;
using Quartz;
using Quartz.Impl.Matchers;

namespace TelegramAntispamBot.Jobs.Base
{
	public class ScheduleInspectorService
	{
		private readonly IScheduler _scheduler;

		public ScheduleInspectorService(IScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		public async Task PrintScheduleInfo()
		{
			var jobGroups = await _scheduler.GetJobGroupNames();

			foreach (var group in jobGroups)
			{
				var jobKeys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group));

				foreach (var jobKey in jobKeys)
				{
					var jobDetail = await _scheduler.GetJobDetail(jobKey);
					var triggers = await _scheduler.GetTriggersOfJob(jobKey);

					Console.WriteLine($"Job: {jobKey.Name}");

					foreach (var trigger in triggers)
					{
						var nextFireTimeUtc = trigger.GetNextFireTimeUtc();
						var nextFireTimeLocal = TimeZoneHelper.ConvertFromUtc(nextFireTimeUtc);

						Console.WriteLine($"  Trigger: {trigger.Key.Name}");
						Console.WriteLine($"  Next fire time (UTC): {nextFireTimeUtc}");
						Console.WriteLine($"  Next fire time (Local): {nextFireTimeLocal}");
						Console.WriteLine($"  Schedule: {(trigger as ICronTrigger)?.CronExpressionString}");
					}
				}
			}
		}
	}
}
