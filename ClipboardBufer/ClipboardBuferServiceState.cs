using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardBufer
{
    internal class ClipboardBuferServiceState
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
