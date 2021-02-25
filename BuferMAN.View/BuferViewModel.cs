using System;
using System.Drawing;
using System.Windows.Forms;

namespace BuferMAN.View
{
    public class BuferViewModel
    {
        public BuferViewModel()
        {
        }

        //public BuferViewModel(string buferText)
        //{
        //    this.OriginBuferText = buferText;
        //}

        public Guid ViewId { get; set; }
        public IDataObject Clip { get; set; }
        public bool Persistent { get; set; }
        public string Alias { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Color DefaultBackColor { get; set; }// Remove color from here
        public object Representation { get; set; }
        public string OriginBuferText { get; set; }
    }
}
