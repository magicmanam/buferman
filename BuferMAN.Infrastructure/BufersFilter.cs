using System;

namespace BuferMAN.Infrastructure
{
    public class BufersFilter
    {
        public BuferType BuferType { get; set; }
        public ClipboardType ClipboardType { get; set; }
        public string Text { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? CreatedAfter { get; set; }
    }
}
