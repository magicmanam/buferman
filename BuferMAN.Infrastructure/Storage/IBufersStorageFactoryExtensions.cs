using BuferMAN.Models;
using System;

namespace BuferMAN.Infrastructure.Storage
{
    public static class IBufersStorageFactoryExtensions
    {
        public static IPersistentBufersStorage CreateStorageByFileExtension(this IBufersStorageFactory factory, string filePath)
        {
            var storageType = filePath.EndsWith("json", StringComparison.InvariantCultureIgnoreCase) ? BufersStorageType.JsonFile :
                                          filePath.EndsWith("txt", StringComparison.InvariantCultureIgnoreCase) ? BufersStorageType.TxtFile :
                                                throw new NotSupportedException("File format is not supported");

            return factory.Create(storageType, filePath);
        }
    }
}
