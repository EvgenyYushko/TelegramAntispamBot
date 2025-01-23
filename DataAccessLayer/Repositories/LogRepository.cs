using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Models;
using DomainLayer.Repositories;
using ServiceLayer.Models;
using static Infrastructure.Common.TimeZoneHelper;

namespace DataAccessLayer.Repositories
{
	public class LogRepository : ILogRepository
	{
		private readonly ApplicationDbContext _dbContext;

		public LogRepository(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public IEnumerable<Log> GetLogs()
		{
			foreach (var log in _dbContext.Logs.OrderByDescending(i => i.DateTime).AsQueryable())
			{
				yield return new Log()
				{
					DateTime = log.DateTime,
					Type = log.Type,
					Message = log.Message,
				};
			}
		}

		public async Task Log(Log log)
		{
			var logEntity = new LogEntity()
			{
				Type = log.Type,
				DateTime = DateTimeNow,
				Message = log.Message,
			};

			_dbContext.Logs.Add(logEntity);
			await _dbContext.SaveChangesAsync();
		}
	}
}
