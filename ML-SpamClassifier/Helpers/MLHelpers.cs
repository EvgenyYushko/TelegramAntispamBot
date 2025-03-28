using System;
using System.IO;

namespace ML_SpamClassifier.Helpers
{
	internal static class MLHelpers
	{
		internal static string _dataSetFileName = "spam_dataset.csv";
		internal static string _dataSetPath = Path.Combine(AppContext.BaseDirectory, _dataSetFileName);
		internal static string _googleFolderId = "1CoUNOXOUgq9S2jv1YkJXfcUf-oIVstC8";
		internal static string _modelFileName = "model.zip";
		internal static string _modelPath = Path.Combine(AppContext.BaseDirectory, _modelFileName);

		public static string GetModelStatus()
		{
			var info = new FileInfo(_modelPath);
			return $"Модель обучена: {info.Exists}\n" +
					$"Размер модели: {info.Length / 1024} KB\n" +
					$"Последнее обновление: {info.LastWriteTime}";
		}
	}
}