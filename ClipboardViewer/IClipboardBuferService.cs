using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardViewer
{
    interface IClipboardBuferService
    {
        IEnumerable<IDataObject> GetClips();

        void AddClip(IDataObject clipDataObject);
        
        IDataObject LastClip { get; }

        bool IsLastClip(IDataObject clipObject);

        IDataObject FirstClip { get; }

        bool Contains(IDataObject clipDataObject);

        void RemoveClip(IDataObject clip);
    }
}
