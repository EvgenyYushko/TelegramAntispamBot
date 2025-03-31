using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLayer.Models;

namespace ServiceLayer.Services.Telegram
{
	public interface IMLService
	{
		public Task UpdateSuspiciousMessages(SuspiciousMessageDto message);
		public Task<List<SuspiciousMessageDto>> GetAllSuspiciousMessages();
		public Task AddSuspiciousMessages(SuspiciousMessageDto message);
		public Task<bool> UpdateDataSet();
		public Task DownloadModel();
		Task UploadModelAndDataSetToDrive();
		Task DeleteReviewedSuspiciousMessages();
	}
}
