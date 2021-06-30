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
        public bool Pinned { get; set; }
 
        /// <summary>
        /// Gets or sets an user alias for bufer UI appearance. Null if not specified.
        /// </summary>
        public string Alias { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Color DefaultBackColor { get; set; }// TODO (m) Remove color from here
        public object Representation { get; set; }
        public string OriginBuferTitle { get; set; }

        /// <summary>
        /// Gets or sets bufer's text representation.
        /// </summary>
        public string TextRepresentation { get; set; }

        public string TextData { get; set; }

        public BuferViewModel ShallowCopy()
        {
            return this.MemberwiseClone() as BuferViewModel;
        }
    }
}
