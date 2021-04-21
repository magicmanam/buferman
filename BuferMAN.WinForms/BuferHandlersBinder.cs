using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.Infrastructure.Files;

namespace BuferMAN.WinForms
{
    internal class BuferHandlersBinder : IBuferHandlersBinder
    {
        private readonly IBuferContextMenuGenerator _buferContextMenuGenerator;
        private readonly IBuferSelectionHandlerFactory _buferSelectionHandlerFactory;
        private readonly IFileStorage _fileStorage;
        private readonly IProgramSettings _settings;

        public BuferHandlersBinder(
            IBuferContextMenuGenerator buferContextMenuGenerator,
            IBuferSelectionHandlerFactory buferSelectionHandlerFactory,
            IFileStorage fileStorage,
            IProgramSettings settings)
        {
            this._buferContextMenuGenerator = buferContextMenuGenerator;
            this._buferSelectionHandlerFactory = buferSelectionHandlerFactory;
            this._fileStorage = fileStorage;
            this._settings = settings;
        }

        public void Bind(IBufer bufer, IBufermanHost bufermanHost)
        {
            new BuferHandlersWrapper(this._buferContextMenuGenerator, this._buferSelectionHandlerFactory, this._fileStorage, bufermanHost, this._settings, bufer);
        }
    }
}
