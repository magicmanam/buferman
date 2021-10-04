using BuferMAN.View;
using magicmanam.UndoRedo;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Clipboard
{
    public interface IClipboardBuferService : IStatefulComponent<ApplicationStateSnapshot>
    {
        IEnumerable<BuferViewModel> GetTemporaryBufers();

        IEnumerable<BuferViewModel> GetPinnedBufers();

        int BufersCount { get; }

        void AddTemporaryClip(BuferViewModel bufer);

        bool TryPinBufer(Guid buferViewId);
        bool TryUnpinBufer(Guid buferViewId);

        BuferViewModel LastTemporaryBufer { get; }
        bool IsLastTemporaryBufer(BuferViewModel bufer);

        bool IsInPinnedBufers(BuferViewModel bufer, out Guid pinnedBuferViewId);

        BuferViewModel FirstTemporaryBufer { get; }

        BuferViewModel FirstPinnedBufer { get; }

        bool IsInTemporaryBufers(BuferViewModel clipDataObject, out Guid viewId);

        void RemoveBufer(Guid buferViewId);

        void RemoveAllBufers();

        void RemovePinnedClips();

        void RemoveTemporaryClips();
    }
}
