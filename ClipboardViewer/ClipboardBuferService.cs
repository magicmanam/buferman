using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ClipboardViewer
{
    class ClipboardBuferService : IClipboardBuferService
    {
        private readonly IList<IDataObject> _dataObjects = new List<IDataObject>();
        private readonly IEqualityComparer<IDataObject> _comparer = new DataObjectComparer();

		public ClipboardBuferService(IEqualityComparer<IDataObject> comparer)
		{
			this._comparer = comparer;
		}

        public IEnumerable<IDataObject> GetClips()
        {
            return _dataObjects.ToList();
        }

        public IDataObject LastClip
        {
            get
            {
                return this._dataObjects.LastOrDefault();
            }
        }
        
        public bool IsLastClip(IDataObject dataObject)
        {
            return this._comparer.Equals(this.LastClip, dataObject);
        }

        public IDataObject FirstClip
        {
            get
            {
                return _dataObjects.FirstOrDefault();
            }
        }

        public bool Contains(IDataObject clip)
        {
            return this._dataObjects.Contains(clip, this._comparer);
        }

        public void RemoveClip(IDataObject clipDataObject)
        {
            var dataObject = this._dataObjects.FirstOrDefault(d => this._comparer.Equals(d, clipDataObject));
            if (dataObject != null)
            {
                this._dataObjects.Remove(dataObject);                
            }
        }

        public void AddClip(IDataObject dataObject)
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
            _dataObjects.Add(copy);
        }
    }
}