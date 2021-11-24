using System.Windows.Forms;
using BuferMAN.Infrastructure;
using BuferMAN.Clipboard;

namespace BuferMAN.ContextMenu
{
    internal class BuferSelectionHandlerFactory : IBuferSelectionHandlerFactory
    {
        private readonly IClipboardWrapper _clipboardWrapper;

        public BuferSelectionHandlerFactory(IClipboardWrapper clipboardWrapper)
        {
            this._clipboardWrapper = clipboardWrapper;
        }

        public IBuferSelectionHandler CreateHandler(IBufer bufer, IBufermanHost bufermanHost)
        {
            return new BuferSelectionHandler(bufer, this._clipboardWrapper, bufermanHost);
        }
    }
}