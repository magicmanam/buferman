using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Menu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class HttpUrlBuferMenuGenerator : IBuferTypeMenuGenerator
    {
        private readonly string _url;

        public HttpUrlBuferMenuGenerator(string url)
        {
            this._url = url;
        }

        public IList<BufermanMenuItem> Generate(IList<BufermanMenuItem> menuItems, IBufermanHost bufermanHost)
        {
            menuItems.Add(bufermanHost.CreateMenuSeparatorItem());

            var openInBrowserMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuOpenInBrowser, (object sender, EventArgs e) =>
                                    Process.Start(this._url));
            openInBrowserMenuItem.ShortCut = Shortcut.CtrlB;
            menuItems.Add(openInBrowserMenuItem);

            return menuItems;
        }
    }
}
