using BuferMAN.View;
using magicmanam.UndoRedo;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Clipboard
{
	public interface IClipboardBuferService : IStatefulComponent<ApplicationStateSnapshot>
    {
		/// <summary>
		/// Returns persistent + temporary clips.
		/// </summary>
		/// <returns></returns>
        IEnumerable<IDataObject> GetClips(bool persistentFirst = false);

        IEnumerable<BuferViewModel> GetTemporaryClips();

        IEnumerable<BuferViewModel> GetPersistentClips();

        int BufersCount { get; }

        void AddTemporaryClip(BuferViewModel bufer);

		bool TryMarkBuferAsPersistent(Guid buferViewId);

        BuferViewModel LastTemporaryBufer { get; }
        bool IsLastTemporaryBufer(BuferViewModel bufer);

		bool IsPersistent(IDataObject clipObject);// IsPinned(BuferViewModel bufer);

		BuferViewModel FirstTemporaryBufer { get; }

        BuferViewModel FirstPersistentBufer { get; }

        bool IsInTemporaryBufers(BuferViewModel clipDataObject);

        void RemoveBufer(Guid buferViewId);

        void RemoveAllBufers();

        void RemovePersistentClips();

        void RemoveTemporaryClips();
    }
}
