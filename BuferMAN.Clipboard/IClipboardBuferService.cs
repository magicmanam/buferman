using BuferMAN.View;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Clipboard
{
	public interface IClipboardBuferService
    {
		/// <summary>
		/// Returns persistent + temporary clips.
		/// </summary>
		/// <returns></returns>
        IEnumerable<IDataObject> GetClips(bool persistentFirst = false);

        IEnumerable<IDataObject> GetTemporaryClips();

        IEnumerable<IDataObject> GetPersistentClips();

        int ClipsCount { get; }

        void AddTemporaryClip(IDataObject clipDataObject);

		bool TryMarkClipAsPersistent(IDataObject dataObject);

        IDataObject LastTemporaryClip { get; }

        bool IsLastTemporaryBufer(BuferViewModel bufer);

		bool IsPersistent(IDataObject clipObject);// IsPinned(BuferViewModel bufer);

		IDataObject FirstTemporaryClip { get; }

        IDataObject FirstPersistentClip { get; }

        bool IsInTemporaryBufers(BuferViewModel clipDataObject);

        void RemoveClip(IDataObject clip);

        void RemoveAllClips();

        void RemovePersistentClips();

        void RemoveTemporaryClips();
    }
}
