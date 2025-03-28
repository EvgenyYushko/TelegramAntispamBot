using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLayer.Models;

namespace DomainLayer.Repositories
{
	public interface ILogRepository
	{
		public IEnumerable<Log> GetLogs();

		public Task Log(Log log);
	}
}