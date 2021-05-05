using BuferMAN.Infrastructure.Files;
using System.IO;

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
            File.Create(filePath).Close();
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}
