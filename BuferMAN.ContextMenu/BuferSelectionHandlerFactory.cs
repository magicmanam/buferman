using System.Windows.Forms;
using BuferMAN.Infrastructure;
using BuferMAN.Clipboard;

namespace BuferMAN.ContextMenu
{
	public class BuferSelectionHandlerFactory : IBuferSelectionHandlerFactory
    {
        private readonly IClipboardWrapper _clipboardWrapper;

        public BuferSelectionHandlerFactory(IClipboardWrapper clipboardWrapper)
        {
            this._clipboardWrapper = clipboardWrapper;
        }

        public IBuferSelectionHandler CreateHandler(IDataObject dataObject)
        {
            return new BuferSelectionHandler(dataObject, this._clipboardWrapper);
        }
    }
}