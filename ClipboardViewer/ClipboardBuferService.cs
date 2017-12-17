using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ClipboardViewer
{
	class ClipboardBuferService : IClipboardBuferService
    {
        private readonly Stack<ClipboardBuferServiceState> _serviceStates = new Stack<ClipboardBuferServiceState>();
        private IList<IDataObject> _tempObjects = new List<IDataObject>();
		private IList<IDataObject> _persistentObjects = new List<IDataObject>();
		private readonly IEqualityComparer<IDataObject> _comparer = new DataObjectComparer();

		public ClipboardBuferService(IEqualityComparer<IDataObject> comparer)
		{
			this._comparer = comparer;
		}

        public IEnumerable<IDataObject> GetClips(bool persistentFirst = false)
        {
            return this._GetAllClips(persistentFirst).ToList();
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
            return this._comparer.Equals(this.LastTemporaryClip, dataObject);
        }

		public bool IsNotPersistent(IDataObject dataObject)
		{
			return !this._persistentObjects.Contains(dataObject, this._comparer);
		}

		public IDataObject FirstClip
        {
            get
            {
                return this._GetAllClips(false).FirstOrDefault();
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
                this._SaveCurrentState();
                list.Remove(dataObject);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void _SaveCurrentState()
        {
            this._serviceStates.Push(new ClipboardBuferServiceState(this._tempObjects.ToList(), this._persistentObjects.ToList()));
        }

        public void AddTemporaryClip(IDataObject dataObject)
        {
            var copy = new DataObject();
            foreach (var format in dataObject.GetFormats())
            {
                if (format == "EnhancedMetafile")//Fixes bug with copy in Word
                {
                    copy.SetData(format, "<< !!! EnhancedMetafile !!! >>");
                }
                else
                {
                    try
                    {
                        copy.SetData(format, dataObject.GetData(format));
                    } catch
                    {
                        //Log input parameters and other details.
                    }
                }
            }
            this._SaveCurrentState();
            this._tempObjects.Add(copy);
        }

		public void MarkClipAsPersistent(IDataObject dataObject)
		{
            this._SaveCurrentState();
			if (this._tempObjects.Remove(dataObject))
			{
				this._persistentObjects.Add(dataObject);
			} else
			{
                this._serviceStates.Pop();
				Logger.Logger.Current.Write("An attempt to mark unexistent object as persistent.");
			}
		}

        public void Undo()
        {
            if (this._serviceStates.Count > 0)
            {
                var lastState = this._serviceStates.Pop();
                this._tempObjects = lastState.TempObjects;
                this._persistentObjects = lastState.PersistentObjects;
            }
        }

        public void CancelUndo()
        {
            throw new NotImplementedException();
        }
    }
}