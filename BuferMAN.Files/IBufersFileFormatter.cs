using BuferMAN.Models;
using System.Collections.Generic;

namespace BuferMAN.Files
{
    public interface IBufersFileFormatter
    {
        IEnumerable<BuferItem> Parse(string content);
        string ToString(IEnumerable<BuferItem> buferItems);
    }
}