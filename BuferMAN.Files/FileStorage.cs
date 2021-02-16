using BuferMAN.Infrastructure.Storage;
using System.IO;

namespace BuferMAN.Files
{
    public class FileStorage : IFileStorage
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
    }
}
