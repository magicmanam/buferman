using BuferMAN.Clipboard.Properties;
using BuferMAN.View;
using magicmanam.UndoRedo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BuferMAN.Clipboard
{
    public class ClipboardBuferService : IClipboardBuferService
    {
        private IList<BuferViewModel> _tempObjects = new List<BuferViewModel>();
		private IList<BuferViewModel> _persistentObjects = new List<BuferViewModel>();
        private readonly IEqualityComparer<IDataObject> _comparer;

        public ClipboardBuferService(IEqualityComparer<IDataObject> comparer)
		{
			this._comparer = comparer;
		}

        public IEnumerable<IDataObject> GetClips(bool persistentFirst = false)
        {
            return this._GetAllClips(persistentFirst).ToList();
        }   

        public int BufersCount { get { return this._tempObjects.Count + this._persistentObjects.Count; } }

        public void RemoveAllBufers()
        {
            if (this._tempObjects.Count + this._persistentObjects.Count > 0)
            {
                using (UndoableContext<ApplicationStateSnapshot>.Current.StartAction(Resource.AllDeleted))
                {
                    this._tempObjects.Clear();
                    this._persistentObjects.Clear();
                }
            }
        }

		private IEnumerable<IDataObject> _GetAllClips(bool persistentFirst)
		{
            return persistentFirst ? this._persistentObjects.Union(this._tempObjects).Select(t => t.Clip) : this._tempObjects.Union(this._persistentObjects).Select(t => t.Clip);
		}

        public BuferViewModel LastTemporaryBufer
        {
            get
            {
                return this._tempObjects.LastOrDefault();
            }
        }

        public bool IsLastTemporaryBufer(BuferViewModel bufer)
        {
            var lastTemporaryBufer = this.LastTemporaryBufer;

            return lastTemporaryBufer != null &&
                string.Equals(lastTemporaryBufer.Alias, bufer.Alias, StringComparison.CurrentCulture) &&
                this._comparer.Equals(lastTemporaryBufer?.Clip, bufer.Clip);
        }

        public bool IsPersistent(BuferViewModel bufer)
		{
			return this._persistentObjects.Any(d => this._comparer.Equals(bufer.Clip, d.Clip) && string.Equals(bufer.Alias, d.Alias, StringComparison.CurrentCulture));
		}

        public BuferViewModel FirstTemporaryBufer
        {
            get
            {
                return this._tempObjects.FirstOrDefault();
            }
        }

        public BuferViewModel FirstPersistentBufer
        {
            get
            {
                return this._persistentObjects.FirstOrDefault();
            }
        }

        public bool IsInTemporaryBufers(BuferViewModel bufer)
        {
            return this._tempObjects.Any(b => string.Equals(b.Alias, bufer.Alias, StringComparison.CurrentCulture) && this._comparer.Equals(bufer.Clip, b.Clip));
        }

        // Maybe add two methods for temp and persistent clips?
        public void RemoveBufer(Guid buferViewId)
        {
            if (this._RemoveClipObject(this._tempObjects, buferViewId) == false)
            {
                if (this._RemoveClipObject(this._persistentObjects, buferViewId) == false)
                {
                    throw new Exception("The clip was not found in temp and persistent collections - unknown situation.");
                }
            }
        }

        private bool _RemoveClipObject(IList<BuferViewModel> list, Guid buferViewId)
        {
            var bufer = list.FirstOrDefault(d => d.ViewId == buferViewId);
            if (bufer != null)
            {
                using (UndoableContext<ApplicationStateSnapshot>.Current.StartAction(Resource.BuferDeleted))
                {
                    list.Remove(bufer);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public ApplicationStateSnapshot UndoableState
        {
            get
            {
                return new ApplicationStateSnapshot(this._tempObjects.Union(this._persistentObjects).ToList());
            }
            set
            {
                this._tempObjects = value.Bufers.Where(b => !b.Persistent).ToList();
                this._persistentObjects = value.Bufers.Where(b => b.Persistent).ToList();
            }
        }

        public void AddTemporaryClip(BuferViewModel bufer)
        {
            using (UndoableContext<ApplicationStateSnapshot>.Current.StartAction(Resource.BuferAdded))
            {
                this._tempObjects.Add(bufer);
            }
        }

        public bool TryMarkBuferAsPersistent(Guid buferViewId)
		{
            using (var operation = UndoableContext<ApplicationStateSnapshot>.Current.StartAction(Resource.BuferPersistent))
            {
                var dataObject = this._tempObjects.FirstOrDefault(d => d.ViewId == buferViewId);
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

        public IEnumerable<BuferViewModel> GetTemporaryClips()
        {
            return this._tempObjects.ToList();
        }

        public IEnumerable<BuferViewModel> GetPersistentClips()
        {
            return this._persistentObjects.ToList();
        }

        public void RemovePersistentClips()
        {
            if (this._persistentObjects.Count > 0)
            {
                using (UndoableContext<ApplicationStateSnapshot>.Current.StartAction(Resource.PersistentBufersDeleted))
                {
                    this._persistentObjects.Clear();
                }
            }
        }

        public void RemoveTemporaryClips()
        {
            if (this._tempObjects.Count > 0)
            {
                using (UndoableContext<ApplicationStateSnapshot>.Current.StartAction(Resource.TemporaryBufersDeleted))
                {
                    this._tempObjects.Clear();
                }
            }
        }
    }
}