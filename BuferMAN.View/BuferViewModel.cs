using System;
using System.Windows.Forms;

namespace BuferMAN.View
{
    public class BuferViewModel
    {
        public BuferViewModel()
        {
            this.CreatedAt = DateTime.Now;
        }
        public IDataObject Clip { get; set; }
        public bool Persistent { get; set; }
        public string Alias { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
