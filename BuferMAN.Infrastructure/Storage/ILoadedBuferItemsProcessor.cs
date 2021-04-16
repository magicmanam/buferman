using BuferMAN.Models;
using System.Collections.Generic;

namespace BuferMAN.Infrastructure.Storage
{
    public interface ILoadedBuferItemsProcessor
    {
        void ProcessBuferItems(IEnumerable<BuferItem> bufers);
    }
}
