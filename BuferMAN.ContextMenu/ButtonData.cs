using System;
using System.Drawing;

namespace BuferMAN.ContextMenu
{
    public class ButtonData
    {
        public ButtonData()
        {
            this.CreatedAt = DateTime.Now;
        }

        public object Representation { get; set; }
        public Color DefaultBackColor { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
