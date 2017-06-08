using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ClipboardViewer
{
	class ClipboardBuferService : IClipboardBuferService
    {
        private readonly IList<IDataObject> _tempObjects = new List<IDataObject>();
		private readonly IList<IDataObject> _persistentObjects = new List<IDataObject>();
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
			if (persistentFirst)
			{
				return this._persistentObjects.Union(this._tempObjects);
			}
			else
			{
				return this._tempObjects.Union(this._persistentObjects);
			}
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
			var dataObject = this._tempObjects.FirstOrDefault(d => this._comparer.Equals(d, clipDataObject));//Only this way
            if (dataObject != null)
            {
                this._tempObjects.Remove(dataObject);                
            } else
			{
				dataObject = this._persistentObjects.FirstOrDefault(d => this._comparer.Equals(d, clipDataObject));
				if (dataObject != null)
				{
					this._persistentObjects.Remove(dataObject);
				}
			}
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
            this._tempObjects.Add(copy);
        }

		public void MarkClipAsPersistent(IDataObject dataObject)
		{
			if (this._tempObjects.Remove(dataObject))
			{
				this._persistentObjects.Add(dataObject);
			} else
			{
				Logger.Logger.Current.Write("An attempt to mark unexistent object as persistent.");
			}
		}

	}
}