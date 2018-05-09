using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Clipboard
{
    public class ClipboardBuferServiceState
    {
        private IList<IDataObject> _tempObjects;
        private IList<IDataObject> _persistentObjects;

        public ClipboardBuferServiceState(IList<IDataObject> tempObjects, IList<IDataObject> persistentObjects)
        {
            this._tempObjects = tempObjects;
            this._persistentObjects = persistentObjects;
        }

        public IList<IDataObject> TempObjects { get { return this._tempObjects; } }

        public IList<IDataObject> PersistentObjects { get { return this._persistentObjects; } }
    }
}
