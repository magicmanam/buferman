using BuferMAN.Clipboard.Properties;
using BuferMAN.View;
using magicmanam.UndoRedo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BuferMAN.Clipboard
{
    public class ClipboardBuferService : IClipboardBuferService, IStatefulComponent<ClipboardBuferServiceState>
    {
        private IList<IDataObject> _tempObjects = new List<IDataObject>();
		private IList<IDataObject> _persistentObjects = new List<IDataObject>();
        private readonly IEqualityComparer<IDataObject> _comparer;

        public ClipboardBuferService(IEqualityComparer<IDataObject> comparer)
		{
			this._comparer = comparer;
		}

        public IEnumerable<IDataObject> GetClips(bool persistentFirst = false)
        {
            return this._GetAllClips(persistentFirst).ToList();
        }   

        public int ClipsCount { get { return this._tempObjects.Count + this._persistentObjects.Count; } }

        public void RemoveAllClips()
        {
            if (this._tempObjects.Count + this._persistentObjects.Count > 0)
            {
                using (UndoableContext<ClipboardBuferServiceState>.Current.StartAction(Resource.AllDeleted))
                {
                    this._tempObjects.Clear();
                    this._persistentObjects.Clear();
                }
            }
        }

		private IEnumerable<IDataObject> _GetAllClips(bool persistentFirst)
		{
            return persistentFirst ? this._persistentObjects.Union(this._tempObjects) : this._tempObjects.Union(this._persistentObjects);
		}

        public IDataObject LastTemporaryClip
        {
            get
            {
                return this._tempObjects.LastOrDefault();
            }
        }
        
        public bool IsLastTemporaryBufer(BuferViewModel bufer)
        {
            return this._comparer.Equals(this.LastTemporaryClip, bufer.Clip);
        }

		public bool IsPersistent(IDataObject dataObject)
		{
			return this._persistentObjects.Contains(dataObject, this._comparer);
		}

        public IDataObject FirstTemporaryClip
        {
            get
            {
                return this._tempObjects.FirstOrDefault();
            }
        }

        public IDataObject FirstPersistentClip
        {
            get
            {
                return this._persistentObjects.FirstOrDefault();
            }
        }

        public bool IsInTemporaryBufers(BuferViewModel bufer)
        {
            return this._tempObjects.Contains(bufer.Clip, this._comparer);
        }

        // Maybe add two methods for temp and persistent clips?
        public void RemoveClip(IDataObject clipDataObject)
        {
            if (this._RemoveClipObject(this._tempObjects, clipDataObject) == false)
            {
                if (this._RemoveClipObject(this._persistentObjects, clipDataObject) == false)
                {
                    throw new Exception("The clip was not found in temp and persistent collections - unknown situation.");
                }
            }
        }

        private bool _RemoveClipObject(IList<IDataObject> list, IDataObject clip)
        {
            var dataObject = list.FirstOrDefault(d => this._comparer.Equals(d, clip));
            if (dataObject != null)
            {
                using (UndoableContext<ClipboardBuferServiceState>.Current.StartAction(Resource.BuferDeleted))
                {
                    list.Remove(dataObject);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public ClipboardBuferServiceState UndoableState
        {
            get
            {
                return new ClipboardBuferServiceState(this._tempObjects.ToList(), this._persistentObjects.ToList());
            }
            set
            {
                this._tempObjects = value.TempObjects;
                this._persistentObjects = value.PersistentObjects;
            }
        }

        public void AddTemporaryClip(IDataObject dataObject)
        {
            using (UndoableContext<ClipboardBuferServiceState>.Current.StartAction(Resource.BuferAdded))
            {
                this._tempObjects.Add(dataObject);
            }
        }

        public bool TryMarkClipAsPersistent(IDataObject clip)
		{
            using (var operation = UndoableContext<ClipboardBuferServiceState>.Current.StartAction(Resource.BuferPersistent))
            {
                var dataObject = this._tempObjects.FirstOrDefault(d => this._comparer.Equals(d, clip));
                if (dataObject != null && this._tempObjects.Remove(dataObject))
                {
                    this._persistentObjects.Add(dataObject);
                    return true;

                }
                else
                {
                    operation.Cancel();
                    return false;
                }
            }
		}

        public IEnumerable<IDataObject> GetTemporaryClips()
        {
            return this._tempObjects.ToList();
        }

        public IEnumerable<IDataObject> GetPersistentClips()
        {
            return this._persistentObjects.ToList();
        }

        public void RemovePersistentClips()
        {
            if (this._persistentObjects.Count > 0)
            {
                using (UndoableContext<ClipboardBuferServiceState>.Current.StartAction(Resource.PersistentBufersDeleted))
                {
                    this._persistentObjects.Clear();
                }
            }
        }

        public void RemoveTemporaryClips()
        {
            if (this._tempObjects.Count > 0)
            {
                using (UndoableContext<ClipboardBuferServiceState>.Current.StartAction(Resource.TemporaryBufersDeleted))
                {
                    this._tempObjects.Clear();
                }
            }
        }
    }
}