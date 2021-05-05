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

        bool FileExists(string filePath);

        IEnumerable<string> GetFiles(string baseDirectory, string searchPattern = "*");
    }
}
