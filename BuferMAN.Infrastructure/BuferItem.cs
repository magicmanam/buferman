using System.Collections.Generic;

namespace BuferMAN.Storage
{
    public class BuferItem
    {
        public object Data { get; set; }
        public IDictionary<string, object> Formats { get; set; }
        public bool IsPersistent { get; set; }
    }
}
