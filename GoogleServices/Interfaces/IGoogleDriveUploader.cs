using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleServices.Interfaces
{
	public interface IGoogleDriveUploader
	{
		Task<string> UploadFileAsync(string localFilePath, string driveFolderId, bool overwrite = false);
		Task<Google.Apis.Drive.v3.Data.File> GetFileByNameAsync(string fileName, string folderId = null);
		Task DownloadFileAsync(string fileId, string localFilePath);
		Task<IList<Google.Apis.Drive.v3.Data.File>> GetAllFilesInFolderAsync(string folderId);
	}
}
