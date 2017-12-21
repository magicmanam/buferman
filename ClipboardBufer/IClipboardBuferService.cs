using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ClipboardBufer
{
	public interface IClipboardBuferService : IUndoable
    {
		/// <summary>
		/// Returns persistent + temporary clips.
		/// </summary>
		/// <returns></returns>
        IEnumerable<IDataObject> GetClips(bool persistentFirst = false);

        void AddTemporaryClip(IDataObject clipDataObject, Image image = null);

		void MarkClipAsPersistent(IDataObject dataObject);

        IDataObject LastTemporaryClip { get; }

        bool IsLastTemporaryClip(IDataObject clipObject);

		bool IsNotPersistent(IDataObject clipObject);

		IDataObject FirstClip { get; }

        bool Contains(IDataObject clipDataObject);

        void RemoveClip(IDataObject clip);
    }
}
