using BuferMAN.Application;
using BuferMAN.Clipboard;
using BuferMAN.ContextMenu;
using BuferMAN.Files;
using BuferMAN.Form;
using BuferMAN.Form.Window;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Infrastructure.Window;
using BuferMAN.Menu;
using BuferMAN.Settings;
using SimpleInjector;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.DI
{
    public class BufermanDIContainer : Container
    {
        public BufermanDIContainer()
        {
            this.Register<IProgramSettings, ProgramSettings>(Lifestyle.Singleton);
            this.Register<IClipboardWrapper, ClipboardWrapper>(Lifestyle.Singleton);
            this.Register<IEqualityComparer<IDataObject>>(() => new DataObjectComparer(ClipboardFormats.StringFormats, ClipboardFormats.FileFormats), Lifestyle.Singleton);
            this.Register<IClipboardBuferService, ClipboardBuferService>(Lifestyle.Singleton);
            this.Register<IBufersFileParser, JsonFileParser>(Lifestyle.Singleton);
            this.Register<IIDataObjectHandler, DataObjectHandler>(Lifestyle.Singleton);
            this.Register<ILoadingFileHandler, LoadingFileHandler>(Lifestyle.Singleton);
            this.Register<IFileStorage, FileStorage>(Lifestyle.Singleton);
            this.Register<IBuferContextMenuGenerator, BuferContextMenuGenerator>(Lifestyle.Singleton);
            this.Register<IBuferSelectionHandlerFactory, BuferSelectionHandlerFactory>(Lifestyle.Singleton);
            this.Register<BuferMANApplication>(Lifestyle.Singleton);
            this.Register<IMainMenuGenerator, MainMenuGenerator>(Lifestyle.Singleton);
            this.Register<IWindowLevelContext, DefaultWindowLevelContext>(Lifestyle.Singleton);
            this.Register<IRenderingHandler, RenderingHandler>(Lifestyle.Singleton);
            this.Register<IBuferHandlersBinder, BuferHandlersBinder>(Lifestyle.Singleton);
        }
    }
}
