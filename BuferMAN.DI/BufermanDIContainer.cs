using BuferMAN.Application;
using BuferMAN.Clipboard;
using BuferMAN.ContextMenu;
using BuferMAN.Files;
using BuferMAN.Menu;
using BuferMAN.Plugins;
using BuferMAN.Settings;
using BuferMAN.Storage;
using SimpleInjector;

namespace BuferMAN.DI
{
    public class BufermanDIContainer : Container
    {
        public BufermanDIContainer()
        {
            this.RegisterClipboardPart()
                .RegisterApplicationPart()
                .RegisterMainMenuPart()
                .RegisterContextMenuPart()
                .RegisterFilesPart()
                .RegisterStoragePart()
                .RegisterSettingsPart()
                .RegisterPlugins();
        }
    }
}
