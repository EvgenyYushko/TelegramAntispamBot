using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using GoogleServices.Interfaces;

namespace GoogleServices.Drive
{
	public class GoogleDriveUploader : IGoogleDriveUploader
	{
		private readonly DriveService _driveService;

		public GoogleDriveUploader(string serviceAccountJson)
		{
			var credential = GoogleCredential.FromJson(serviceAccountJson)
				.CreateScoped(DriveService.Scope.DriveFile);

			_driveService = new DriveService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = "Github Actions Backup Uploader"
			});
		}

		public async Task<string> UploadFileAsync(string localFilePath, string driveFolderId, bool overwrite = false)
		{
			var fileName = Path.GetFileName(localFilePath);

			// Если включена перезапись, ищем существующий файл
			if (overwrite)
			{
				var existingFile = await GetFileByNameAsync(fileName, driveFolderId);
				if (existingFile != null)
				{
					// Удаляем существующий файл
					await _driveService.Files.Delete(existingFile.Id).ExecuteAsync();
				}
			}

			// Загружаем новый файл
			var fileMetadata = new Google.Apis.Drive.v3.Data.File()
			{
				Name = fileName,
				Parents = new[] { driveFolderId }
			};

			using var fileStream = new FileStream(localFilePath, FileMode.Open);
			var request = _driveService.Files.Create(
				fileMetadata,
				fileStream,
				GetMimeType(localFilePath));

			request.Fields = "id";
			var result = await request.UploadAsync(CancellationToken.None);

			if (result.Status != Google.Apis.Upload.UploadStatus.Completed)
				throw new Exception("Upload failed: " + result.Exception);

			return request.ResponseBody.Id;
		}

		public async Task<Google.Apis.Drive.v3.Data.File> GetFileByNameAsync(string fileName, string folderId = null)
		{
			// Создаем запрос для поиска файла по имени
			var listRequest = _driveService.Files.List();
			listRequest.Q = $"name = '{fileName}'";

			// Если указана папка, ищем файл только в этой папке
			if (!string.IsNullOrEmpty(folderId))
			{
				listRequest.Q += $" and '{folderId}' in parents";
			}

			listRequest.Fields = "files(id, name, mimeType, size, createdTime)";

			// Выполняем запрос
			var result = await listRequest.ExecuteAsync();

			// Возвращаем первый найденный файл (или null, если файл не найден)
			return result.Files.FirstOrDefault();
		}

		public async Task DownloadFileAsync(string fileId, string localFilePath)
		{
			var request = _driveService.Files.Get(fileId);
			using var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);
			await request.DownloadAsync(fileStream);
		}

		private string GetMimeType(string fileName)
		{
			var extension = Path.GetExtension(fileName).ToLower();
			return extension switch
			{
				".sql" => "application/sql",
				".gz" => "application/gzip",
				".zip" => "application/zip",
				_ => "application/octet-stream"
			};
		}
	}
}
