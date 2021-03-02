using System.Collections.Generic;

namespace BuferMAN.Models
{
    public class BuferItem
    {
        public object Data { get; set; }
        public string Text { get; set; }
        public IDictionary<string, object> Formats { get; set; }
        public bool Pinned { get; set; }
        public string Alias { get; set; }
    }
}
