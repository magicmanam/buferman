using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.View;
using System.Windows.Forms;

namespace BuferMAN.Form
{
    public class BuferHandlersBinder : IBuferHandlersBinder
    {
        private readonly IBuferContextMenuGenerator _buferContextMenuGenerator;
        private readonly IBuferSelectionHandlerFactory _buferSelectionHandlerFactory;
        private readonly IFileStorage _fileStorage;

        public BuferHandlersBinder(IBuferContextMenuGenerator buferContextMenuGenerator, IBuferSelectionHandlerFactory buferSelectionHandlerFactory, IFileStorage fileStorage)
        {
            this._buferContextMenuGenerator = buferContextMenuGenerator;
            this._buferSelectionHandlerFactory = buferSelectionHandlerFactory;
            this._fileStorage = fileStorage;
        }

        public void Bind(BuferViewModel buferViewModel, Button button, IBufer bufer, IBufermanHost bufermanHost)
        {
            new BuferHandlersWrapper(buferViewModel, button, this._buferContextMenuGenerator, this._buferSelectionHandlerFactory, this._fileStorage, bufermanHost, bufer);
        }
    }
}
