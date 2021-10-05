using System;

namespace BuferMAN.Infrastructure
{
    public class BufersFilter
    {
        public BuferType BuferType { get; set; }
        public ClipboardType ClipboardType { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
