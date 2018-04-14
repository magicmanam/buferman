using magicmanam.UndoableOperations;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Clipboard
{
	public interface IClipboardBuferService : IUndoable
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

		bool MarkClipAsPersistent(IDataObject dataObject);

        IDataObject LastTemporaryClip { get; }

        bool IsLastTemporaryClip(IDataObject clipObject);

		bool IsNotPersistent(IDataObject clipObject);

		IDataObject FirstTemporaryClip { get; }

        IDataObject FirstPersistentClip { get; }

        bool Contains(IDataObject clipDataObject);

        void RemoveClip(IDataObject clip);

        void RemoveAllClips();

        void RemovePersistentClips();

        void RemoveTemporaryClips();
    }
}
