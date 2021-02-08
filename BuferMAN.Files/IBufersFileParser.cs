using BuferMAN.Storage;
using System.Collections.Generic;
using System.IO;

namespace BuferMAN.Files
{
    public interface IBufersFileParser
    {
        IEnumerable<BuferItem> Parse(StreamReader reader);
    }
}