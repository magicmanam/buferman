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

        IEnumerable<BuferViewModel> GetPinnedBufers();

        int BufersCount { get; }

        void AddTemporaryClip(BuferViewModel bufer);

		bool TryPinBufer(Guid buferViewId);

        BuferViewModel LastTemporaryBufer { get; }
        bool IsLastTemporaryBufer(BuferViewModel bufer);

		bool IsPinned(BuferViewModel bufer);

		BuferViewModel FirstTemporaryBufer { get; }

        BuferViewModel FirstPinnedBufer { get; }

        bool IsInTemporaryBufers(BuferViewModel clipDataObject);

        void RemoveBufer(Guid buferViewId);

        void RemoveAllBufers();

        void RemovePinnedClips();

        void RemoveTemporaryClips();
    }
}
