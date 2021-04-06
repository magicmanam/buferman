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
		private IList<BuferViewModel> _pinnedObjects = new List<BuferViewModel>();
        private readonly IEqualityComparer<IDataObject> _comparer;

        public ClipboardBuferService(IEqualityComparer<IDataObject> comparer)
		{
			this._comparer = comparer;
		}

        public IEnumerable<IDataObject> GetClips(bool pinnedFirst = false)
        {
            return this._GetAllClips(pinnedFirst).ToList();
        }   

        public int BufersCount { get { return this._tempObjects.Count + this._pinnedObjects.Count; } }

        public void RemoveAllBufers()
        {
            if (this._tempObjects.Count + this._pinnedObjects.Count > 0)
            {
                using (UndoableContext<ApplicationStateSnapshot>.Current.StartAction(Resource.AllDeleted))
                {
                    this._tempObjects.Clear();
                    this._pinnedObjects.Clear();
                }
            }
        }

		private IEnumerable<IDataObject> _GetAllClips(bool pinnedFirst)
		{
            return pinnedFirst ? this._pinnedObjects.Union(this._tempObjects).Select(t => t.Clip) : this._tempObjects.Union(this._pinnedObjects).Select(t => t.Clip);
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

        public bool IsInPinnedBufers(BuferViewModel bufer, out Guid pinnedBuferViewId)
		{
			var pinnedBufer = this._pinnedObjects.FirstOrDefault(d => this._comparer.Equals(bufer.Clip, d.Clip) && string.Equals(bufer.Alias, d.Alias, StringComparison.CurrentCulture));

            if (pinnedBufer != null)
            {
                pinnedBuferViewId = pinnedBufer.ViewId;
                return true;
            }
            else
            {
                pinnedBuferViewId = Guid.Empty;
                return false;
            }
		}

        public BuferViewModel FirstTemporaryBufer
        {
            get
            {
                return this._tempObjects.FirstOrDefault();
            }
        }

        public BuferViewModel FirstPinnedBufer
        {
            get
            {
                return this._pinnedObjects.FirstOrDefault();
            }
        }

        public bool IsInTemporaryBufers(BuferViewModel bufer, out Guid viewId)
        {
            var tempBufer = this._tempObjects.FirstOrDefault(b => string.Equals(b.Alias, bufer.Alias, StringComparison.CurrentCulture) && this._comparer.Equals(bufer.Clip, b.Clip));

            viewId = tempBufer != null ? tempBufer.ViewId : Guid.Empty;

            return tempBufer != null;
        }

        // Maybe add two methods for temp and persistent clips?
        public void RemoveBufer(Guid buferViewId)
        {
            if (this._RemoveClipObject(this._tempObjects, buferViewId) == false)
            {
                if (this._RemoveClipObject(this._pinnedObjects, buferViewId) == false)
                {
                    throw new Exception("The clip was not found in temp and pinned collections - unknown situation.");
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
                return new ApplicationStateSnapshot(this._tempObjects.Union(this._pinnedObjects)
                    .Select(o => o.ShallowCopy())
                    .ToList());
            }
            set
            {
                this._tempObjects = value.Bufers.Where(b => !b.Pinned).ToList();
                this._pinnedObjects = value.Bufers.Where(b => b.Pinned).ToList();
            }
        }

        public void AddTemporaryClip(BuferViewModel bufer)
        {
            using (UndoableContext<ApplicationStateSnapshot>.Current.StartAction(Resource.BuferAdded))
            {
                this._tempObjects.Add(bufer);
            }
        }

        public bool TryPinBufer(Guid buferViewId)
        {
            using (var operation = UndoableContext<ApplicationStateSnapshot>.Current.StartAction(Resource.BuferPinned))
            {
                var dataObject = this._tempObjects.FirstOrDefault(d => d.ViewId == buferViewId);
                if (dataObject != null && this._tempObjects.Remove(dataObject))
                {
                    this._pinnedObjects.Add(dataObject);
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

        public IEnumerable<BuferViewModel> GetPinnedBufers()
        {
            return this._pinnedObjects.ToList();
        }

        public void RemovePinnedClips()
        {
            if (this._pinnedObjects.Count > 0)
            {
                using (UndoableContext<ApplicationStateSnapshot>.Current.StartAction(Resource.PinnedBufersDeleted))
                {
                    this._pinnedObjects.Clear();
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