using System;
using System.IO;

namespace Web.UI.Helpers {
	public static class Heplers {

		public static string SaveFile(string uniqueFileName, string extension, byte[] file, string path,
		  string prefix = null) {
			if (uniqueFileName == null) throw new ArgumentNullException("uniqueFileName");
			if (extension == null) throw new ArgumentNullException("extension");
			if (file == null) throw new ArgumentNullException("file");
			if (path == null) throw new ArgumentNullException("path");

			if (extension.StartsWith("."))
				extension = extension.TrimStart(new[] { '.' });

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			string fileName;
			

			using (
				var newFile =
					File.Create(string.Format("{0}/{1}.{2}", path,  uniqueFileName, extension),
						file.Length)) {
				newFile.Write(file, 0, file.Length);
				fileName = Path.GetFileName(newFile.Name);
			}

			return fileName;
		}

		public static string GetFileUrl(byte[] image, string fileName, string rootPath, string itemPath,
			string extension ) {
			string fileUrl = null;
			if (image != null) {
				
				var combinedPath = string.Format("{0}/{1}", rootPath, itemPath);
				fileUrl = string.Format("{0}/{1}", combinedPath,
					SaveFile(fileName, extension, image, combinedPath ));
			}
			return fileUrl;
		}
	}
}