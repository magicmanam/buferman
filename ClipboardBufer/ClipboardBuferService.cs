using ClipboardBufer.Properties;
using magicmanam.UndoableOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ClipboardBufer
{
    public class ClipboardBuferService : IClipboardBuferService
    {
        private readonly Stack<ClipboardBuferServiceState> _serviceStates = new Stack<ClipboardBuferServiceState>();
        private readonly Stack<ClipboardBuferServiceState> _undoableStates = new Stack<ClipboardBuferServiceState>();
        private IList<IDataObject> _tempObjects = new List<IDataObject>();
		private IList<IDataObject> _persistentObjects = new List<IDataObject>();
		private readonly IEqualityComparer<IDataObject> _comparer;

        public ClipboardBuferService(IEqualityComparer<IDataObject> comparer)
		{
			this._comparer = comparer;
		}

        public event EventHandler<UndoableActionEventArgs> UndoableAction;
        public event EventHandler<UndoableActionEventArgs> UndoAction;
        public event EventHandler<UndoableActionEventArgs> CancelUndoAction;
        public event EventHandler<UndoableContextChangedEventArgs> UndoableContextChanged;

        protected virtual void OnUndoableAction(string action)
        {
            this._undoableStates.Clear();
            this.UndoableAction?.Invoke(this, new UndoableActionEventArgs(action));
            this.UndoableContextChanged?.Invoke(this, new UndoableContextChangedEventArgs(true, false));
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
                this._serviceStates.Push(this._GetCurrentState());
                this._tempObjects.Clear();
                this._persistentObjects.Clear();
                this.OnUndoableAction(Resource.AllDeleted);
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
        
        public bool IsLastTemporaryClip(IDataObject dataObject)
        {
            return dataObject.GetFormats().Any() && this._comparer.Equals(this.LastTemporaryClip, dataObject);
        }

		public bool IsNotPersistent(IDataObject dataObject)
		{
			return !this._persistentObjects.Contains(dataObject, this._comparer);
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

        public bool Contains(IDataObject clip)
        {
            return this._GetAllClips(false).Contains(clip, this._comparer);
        }

        public void RemoveClip(IDataObject clipDataObject)
        {
            if (this._RemoveClipObject(this._tempObjects, clipDataObject) == false)
            {
                this._RemoveClipObject(this._persistentObjects, clipDataObject);
            }
        }

        private bool _RemoveClipObject(IList<IDataObject> list, IDataObject clip)
        {
            var dataObject = list.FirstOrDefault(d => this._comparer.Equals(d, clip));
            if (dataObject != null)
            {
                this._serviceStates.Push(this._GetCurrentState());
                list.Remove(dataObject);
                this.OnUndoableAction(Resource.BuferDeleted);
                return true;
            }
            else
            {
                return false;
            }
        }

        private ClipboardBuferServiceState _GetCurrentState()
        {
            return new ClipboardBuferServiceState(this._tempObjects.ToList(), this._persistentObjects.ToList());
        }

        public void AddTemporaryClip(IDataObject dataObject)
        {
            this._serviceStates.Push(this._GetCurrentState());
            this._tempObjects.Add(dataObject);
            this.OnUndoableAction(Resource.BuferAdded);
        }

        public void MarkClipAsPersistent(IDataObject dataObject)
		{
            this._serviceStates.Push(this._GetCurrentState());
            if (this._tempObjects.Remove(dataObject))
			{
				this._persistentObjects.Add(dataObject);
                this.OnUndoableAction(Resource.BuferPersistent);

            } else
			{
                this._serviceStates.Pop();
			}
		}

        public void Undo()
        {
            if (this._serviceStates.Count > 0)
            {
                this._undoableStates.Push(this._GetCurrentState());

                var lastState = this._serviceStates.Pop();
                this._tempObjects = lastState.TempObjects;
                this._persistentObjects = lastState.PersistentObjects;
                this.UndoAction?.Invoke(this, new UndoableActionEventArgs(Resource.BuferOperationCancelled));
                this.UndoableContextChanged?.Invoke(this, new UndoableContextChangedEventArgs(this._serviceStates.Any(), true));
            }
        }

        public void CancelUndo()
        {
            if (this._undoableStates.Count > 0)
            {
                this._serviceStates.Push(this._GetCurrentState());

                var undoState = this._undoableStates.Pop();
                this._tempObjects = undoState.TempObjects;
                this._persistentObjects = undoState.PersistentObjects;
                this.CancelUndoAction?.Invoke(this, new UndoableActionEventArgs(Resource.BuferOperationRestored));
                this.UndoableContextChanged?.Invoke(this, new UndoableContextChangedEventArgs(true, this._undoableStates.Any()));
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
                this._serviceStates.Push(this._GetCurrentState());
                this._persistentObjects.Clear();
                this.OnUndoableAction(Resource.PersistentBufersDeleted);
            }
        }

        public void RemoveTemporaryClips()
        {
            if (this._tempObjects.Count > 0)
            {
                this._serviceStates.Push(this._GetCurrentState());
                this._tempObjects.Clear();
                this.OnUndoableAction(Resource.TemporaryBufersDeleted);
            }
        }
    }
}