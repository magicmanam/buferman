using BuferMAN.Infrastructure.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BuferMAN.Files
{
    internal class FileStorage : IFileStorage
    {
        public string GetFileDirectory(string filePath)
        {
            return Path.GetDirectoryName(filePath);
        }

        public string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        public FileAttributes GetFileAttributes(string filePath)
        {
            return File.GetAttributes(filePath);
        }

        public void CreateFile(string filePath)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.Create(filePath).Close();
        }

        public void CreateDirectory(string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public bool DirectoryExists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        public IEnumerable<string> GetFiles(string baseDirectory, string searchPattern = "*")
        {
            return Directory.GetFiles(baseDirectory, searchPattern).ToList();
        }

        public string DataDirectory => Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    Application.ProductName);

        public string CombinePaths(params string[] paths)
        {
            return Path.Combine(paths);
        }


    }
}
