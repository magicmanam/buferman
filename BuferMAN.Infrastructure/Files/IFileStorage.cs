using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuferMAN.Infrastructure.Files
{
    public interface IFileStorage
    {
        string GetFileDirectory(string filePath);

        string GetFileName(string filePath);

        FileAttributes GetFileAttributes(string filePath);
    }
}
