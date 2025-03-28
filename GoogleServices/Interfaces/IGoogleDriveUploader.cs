using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Drive.v3.Data;

namespace GoogleServices.Interfaces
{
	public interface IGoogleDriveUploader
	{
		Task<string> UploadFileAsync(string localFilePath, string driveFolderId, bool overwrite = false);

		Task<File> GetFileByNameAsync(string fileName, string folderId = null);

		Task DownloadFileAsync(string fileId, string localFilePath);

		Task<IList<File>> GetAllFilesInFolderAsync(string folderId);

		Task DeleteFileAcync(string fileName, string driveFolderId);
	}
}