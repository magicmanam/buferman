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
    }
}
