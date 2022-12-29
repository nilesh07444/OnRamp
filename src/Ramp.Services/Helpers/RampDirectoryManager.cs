using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Ramp.Services.Helpers
{
    public static class RampDirectoryManager
    {
        public static void UploadMultipleFiles(string directoryPath, IList<HttpPostedFileBase> fileList)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            if (fileList[0] != null)
            {
                foreach (HttpPostedFileBase fileBeingUploaded in fileList)
                {
                    HttpPostedFileBase httpPostedFileBase = fileBeingUploaded;
                    string combine = Path.Combine(directoryPath, fileBeingUploaded.FileName);
                    httpPostedFileBase.SaveAs(combine);
                }
            }
        }

        public static void UploadSingleFile(string directoryPath, HttpPostedFileBase file)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            if (file != null)
            {
                HttpPostedFileBase httpPostedFileBase = file;
                string combine = Path.Combine(directoryPath, file.FileName);
                httpPostedFileBase.SaveAs(combine);
            }
        }

        public static void UploadFileByGivenName(string directoryPath, HttpPostedFileBase file, string fileName)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            if (file != null)
            {
                HttpPostedFileBase httpPostedFileBase = file;
                string combine = Path.Combine(directoryPath, fileName);
                httpPostedFileBase.SaveAs(combine);
            }
        }

        public static IEnumerable<string> RetriveAllFiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return null;
            }

            string[] filePaths = Directory.GetFiles(directoryPath);
            return filePaths;
        }

        public static string RetriveSingleFile(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return null;
            }

            string[] filePaths = Directory.GetFiles(directoryPath);
            if (filePaths.Length > 0)
            {
                return filePaths[0];
            }
            return null;
        }

        public static void DeleteFile(string directoryPath)
        {
            if (directoryPath != null)
            {
                if (File.Exists(directoryPath))
                {
                    File.Delete(directoryPath);
                }
            }
        }

        public static void DeleteDirectoryAndContents(string directoryPath)
        {
            if (directoryPath != null)
            {
                var directoryInfo = new DirectoryInfo(directoryPath);

                if (directoryInfo.Exists)
                {
                    directoryInfo.Delete(true);
                }
            }
        }

        public static void DeleteDirectoryContentsLeavingDirectoryIntact(string directoryPath)
        {
            if (directoryPath != null)
            {
                if (Directory.Exists(directoryPath))
                {
                    string[] files = Directory.GetFiles(directoryPath);
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                }
            }
        }

        public static void MoveFile(string sourceDirectoryPath, string destinationDirectoryPath, string fileName)
        {
            string sourceFilePath = sourceDirectoryPath + "\\" + fileName;
            string destinationFilePath = destinationDirectoryPath + "\\" + fileName;
            if (sourceDirectoryPath != null && destinationDirectoryPath != null)
            {
                if (Directory.Exists(sourceDirectoryPath) && Directory.Exists(destinationDirectoryPath))
                {
                    File.Copy(sourceFilePath, destinationFilePath);
                    File.Delete(sourceFilePath);
                }
            }
        }

        public static void MoveDirectory(string sourceDirectoryPath, string destinationDirectoryPath)
        {
            if (sourceDirectoryPath != null && destinationDirectoryPath != null)
            {
                try
                {
                    var directoryInfo = new DirectoryInfo(sourceDirectoryPath);
                    directoryInfo.MoveTo(destinationDirectoryPath);
                }
                catch (Exception)
                {
                    Directory.Move(sourceDirectoryPath, destinationDirectoryPath);
                }
            }
        }

        public static bool CreateDirectory(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
                return true;
            }

            return false;
        }

        public static void CreateProductAndRecieptImagesDirectory(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
                Directory.CreateDirectory(path + ConfigurationManager.AppSettings["ProductViewImagesFolderName"]);
                Directory.CreateDirectory(path + ConfigurationManager.AppSettings["ReceiptImagesFolderName"]);
            }
        }

        public static void CleanUpQuestionFiles(string QuestionDirectoryPath, IEnumerable<string> itemsToKeep)
        {
            if (!string.IsNullOrEmpty(QuestionDirectoryPath) && itemsToKeep != null)
            {
                if (Directory.Exists(QuestionDirectoryPath))
                {
                    var files = Directory.GetFiles(QuestionDirectoryPath);
                    var filesToDelete = files.Where(f => !itemsToKeep.Contains(Path.GetFileName(f)));

                    foreach (var FilePath in filesToDelete)
                        if (File.Exists(FilePath))
                            File.Delete(FilePath);
                }
            }
        }

        public static Dictionary<FileTypes, List<string>> FindValidFiles(string DirectoryPath, bool checkImages, bool checkVideo)
        {
            if (Directory.Exists(DirectoryPath))
            {
                var files = Directory.GetFiles(DirectoryPath);
                var result = new Dictionary<FileTypes, List<string>>();

                if (checkImages)
                {
                    var jpegFiles = files.Where(f => Path.GetExtension(f).Equals(".jpeg"));
                    var pngFiles = files.Where(f => Path.GetExtension(f).Equals(".png"));
                    var gifFiles = files.Where(f => Path.GetExtension(f).Equals(".gif"));
                    var bmpFiles = files.Where(f => Path.GetExtension(f).Equals(".bmp"));
                    var jpgFiles = files.Where(f => Path.GetExtension(f).Equals(".jpg"));

                    if (jpegFiles.Any())
                        result.Add(FileTypes.jpeg, jpegFiles.ToList());
                    if (pngFiles.Any())
                        result.Add(FileTypes.png, pngFiles.ToList());
                    if (gifFiles.Any())
                        result.Add(FileTypes.gif, gifFiles.ToList());
                    if (bmpFiles.Any())
                        result.Add(FileTypes.bmp, bmpFiles.ToList());
                    if (jpgFiles.Any())
                        result.Add(FileTypes.jpg, jpgFiles.ToList());
                }
                if (checkVideo)
                {
                    var mp4Files = files.Where(f => Path.GetExtension(f).Equals(".mp4"));
                    var aviFiles = files.Where(f => Path.GetExtension(f).Equals(".avi"));
                    var wmvFiles = files.Where(f => Path.GetExtension(f).Equals(".wmv"));
                    var movFiles = files.Where(f => Path.GetExtension(f).Equals(".mov"));
                    var mpegFiles = files.Where(f => Path.GetExtension(f).Equals(".mpeg"));
                    var threegpFiles = files.Where(f => Path.GetExtension(f).Equals(".3gp"));

                    if (mp4Files.Any())
                        result.Add(FileTypes.mp4, mp4Files.ToList());
                    if (aviFiles.Any())
                        result.Add(FileTypes.avi, aviFiles.ToList());
                    if (wmvFiles.Any())
                        result.Add(FileTypes.wmv, wmvFiles.ToList());
                    if (movFiles.Any())
                        result.Add(FileTypes.mov, movFiles.ToList());
                    if (mpegFiles.Any())
                        result.Add(FileTypes.mpeg, mpegFiles.ToList());
                    if (threegpFiles.Any())
                        result.Add(FileTypes.threeGP, threegpFiles.ToList());
                }
                return result;
            }
            return null;
        }
    }

    public class RampLocationManager
    {
        private Dictionary<FileDirectoryLocations, string> manager = new Dictionary<FileDirectoryLocations, string>(5);
        public const string TempDirectoryName = "TemporaryFiles";

        public RampLocationManager()
        {
        }

        public RampLocationManager(string UploadedFilesPath)
        {
            UploadedFiles = UploadedFilesPath;
        }

        public RampLocationManager(string UploadedfilesPath, string CompanyName)
        {
            UploadedFiles = UploadedfilesPath;
            CompanyRoot = Path.Combine(UploadedfilesPath, CompanyName);
        }

        public RampLocationManager(string UploadedfilesPath, string CompanyName, Guid GuideId)
        {
            UploadedFiles = UploadedfilesPath;
            CompanyRoot = Path.Combine(UploadedfilesPath, CompanyName);
            GuideRoot = Path.Combine(CompanyRoot, GuideId.ToString());
        }

        public RampLocationManager(string UploadedfilesPath, string CompanyName, Guid GuideId, Guid TestId)
        {
            UploadedFiles = UploadedfilesPath;
            CompanyRoot = Path.Combine(UploadedfilesPath, CompanyName);
            GuideRoot = Path.Combine(CompanyRoot, GuideId.ToString());
            TestRoot = Path.Combine(GuideRoot, TestId.ToString());
        }

        public RampLocationManager(string UploadedfilesPath, string TempDirectoryName, string currentyLoggedInUser)
        {
            UploadedFiles = UploadedfilesPath;
            TempRoot = Path.Combine(UploadedfilesPath, TempDirectoryName, currentyLoggedInUser);
        }

        public string UploadedFiles
        {
            get
            {
                if (string.IsNullOrEmpty(manager[FileDirectoryLocations.UploadedFiles]))
                    throw new ArgumentNullException();
                return manager[FileDirectoryLocations.UploadedFiles];
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (manager.ContainsKey(FileDirectoryLocations.UploadedFiles))
                        manager[FileDirectoryLocations.UploadedFiles] = value;
                    else
                        manager.Add(FileDirectoryLocations.UploadedFiles, value);
                }
            }
        }

        public string CompanyRoot
        {
            get
            {
                if (string.IsNullOrEmpty(manager[FileDirectoryLocations.CompanyRoot]))
                    throw new ArgumentNullException();
                return manager[FileDirectoryLocations.CompanyRoot];
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (manager.ContainsKey(FileDirectoryLocations.CompanyRoot))
                        manager[FileDirectoryLocations.CompanyRoot] = value;
                    else
                        manager.Add(FileDirectoryLocations.CompanyRoot, value);
                }
            }
        }

        public string GuideRoot
        {
            get
            {
                if (string.IsNullOrEmpty(manager[FileDirectoryLocations.GuideRoot]))
                    throw new ArgumentNullException();
                return manager[FileDirectoryLocations.GuideRoot];
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (manager.ContainsKey(FileDirectoryLocations.GuideRoot))
                        manager[FileDirectoryLocations.GuideRoot] = value;
                    else
                        manager.Add(FileDirectoryLocations.GuideRoot, value);
                }
            }
        }

        public string TestRoot
        {
            get
            {
                if (string.IsNullOrEmpty(manager[FileDirectoryLocations.TestRoot]))
                    throw new ArgumentNullException();
                return manager[FileDirectoryLocations.TestRoot];
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (manager.ContainsKey(FileDirectoryLocations.TestRoot))
                        manager[FileDirectoryLocations.TestRoot] = value;
                    else
                        manager.Add(FileDirectoryLocations.TestRoot, value);
                }
            }
        }

        public string TempRoot
        {
            get
            {
                if (string.IsNullOrEmpty(manager[FileDirectoryLocations.TempRoot]))
                    throw new ArgumentNullException();
                return manager[FileDirectoryLocations.TempRoot];
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (manager.ContainsKey(FileDirectoryLocations.TempRoot))
                        manager[FileDirectoryLocations.TempRoot] = value;
                    else
                        manager.Add(FileDirectoryLocations.TempRoot, value);
                }
            }
        }

        public bool doCreateUploadedFiles()
        {
            try
            {
                RampDirectoryManager.CreateDirectory(UploadedFiles);
                return true;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        public bool doCreateToCompanyRoot()
        {
            try
            {
                RampDirectoryManager.CreateDirectory(UploadedFiles);
                RampDirectoryManager.CreateDirectory(CompanyRoot);
                return true;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        public bool doCreateToGuideRoot()
        {
            try
            {
                RampDirectoryManager.CreateDirectory(UploadedFiles);
                RampDirectoryManager.CreateDirectory(CompanyRoot);
                RampDirectoryManager.CreateDirectory(GuideRoot);
                return true;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        public bool doCreateToTestRoot()
        {
            try
            {
                RampDirectoryManager.CreateDirectory(UploadedFiles);
                RampDirectoryManager.CreateDirectory(CompanyRoot);
                RampDirectoryManager.CreateDirectory(GuideRoot);
                RampDirectoryManager.CreateDirectory(TestRoot);
                return true;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        public bool doCreateTempRoot()
        {
            try
            {
                RampDirectoryManager.CreateDirectory(UploadedFiles);
                RampDirectoryManager.CreateDirectory(Path.Combine(UploadedFiles, TempDirectoryName));
                RampDirectoryManager.CreateDirectory(TempRoot);
                return true;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }
    }

    public enum FileDirectoryLocations
    {
        UploadedFiles,
        CompanyRoot,
        TestRoot,
        GuideRoot,
        TempRoot,
    }

    public enum FileTypes
    {
        jpeg,
        jpg,
        png,
        gif,
        bmp,
        mp4,
        avi,
        wmv,
        mov,
        mpeg,
        threeGP,
    }
}