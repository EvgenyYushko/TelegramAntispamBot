using System;
using System.Threading.Tasks;
using Infrastructure.Common;
using Quartz;
using Quartz.Impl.Matchers;
using static Infrastructure.Helpers.Logger;

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

					Log($"Job: {jobKey.Name}");

					foreach (var trigger in triggers)
					{
						var nextFireTimeUtc = trigger.GetNextFireTimeUtc();
						var nextFireTimeLocal = TimeZoneHelper.ConvertFromUtc(nextFireTimeUtc);

						Log($"  Trigger: {trigger.Key.Name}");
						//Log($"  Next fire time (UTC): {nextFireTimeUtc}");
						Log($"  Next fire time (Local): {nextFireTimeLocal}");
						Log($"  Schedule: {(trigger as ICronTrigger)?.CronExpressionString}");
					}
				}
			}
		}
	}
}
