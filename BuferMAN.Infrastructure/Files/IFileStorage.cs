using System.Collections.Generic;
using System.IO;

namespace BuferMAN.Infrastructure.Files
{
    public interface IFileStorage
    {
        string GetFileDirectory(string filePath);

        string GetFileName(string filePath);

        FileAttributes GetFileAttributes(string filePath);

        void CreateFile(string filePath);

        void CreateDirectory(string directoryPath);

        bool FileExists(string filePath);

        bool DirectoryExists(string directoryPath);

        IEnumerable<string> GetFiles(string baseDirectory, string searchPattern = "*");
    }
}
