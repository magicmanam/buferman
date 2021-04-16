using BuferMAN.Infrastructure;
using BuferMAN.Models;
using System.Collections.Generic;

namespace BuferMAN.Settings
{
    public class ProgramSettings : IProgramSettings
    {
        public IEnumerable<BufersStorageModel> StoragesToLoadOnStart =>
            new List<BufersStorageModel> {
                new BufersStorageModel
                {
                    StorageType = BufersStorageType.JsonFile,
                    StorageAddress = this.DefaultBufersFileName
                }
            };

        public string DefaultBufersFileName => "bufers.json";

        public int MaxBufersCount => 30;

        public int ExtraBufersCount => 25;
    }
}
