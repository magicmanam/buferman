using BuferMAN.View;
using System.Collections.Generic;

namespace BuferMAN.Clipboard
{
    public class ApplicationStateSnapshot // TODO: replace from this assembly with ClipboardBuferService
    {
        public ApplicationStateSnapshot(IList<BuferViewModel> bufers)
        {
            this.Bufers = bufers;
        }

        public IList<BuferViewModel> Bufers { get; }
    }
}
