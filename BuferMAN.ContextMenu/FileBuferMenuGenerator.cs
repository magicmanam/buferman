using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Menu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class FileBuferMenuGenerator : IBuferTypeMenuGenerator
    {
        private readonly string _filePath;

        public FileBuferMenuGenerator(string filePath)
        {
            this._filePath = filePath;
        }

        public IList<BufermanMenuItem> Generate(IList<BufermanMenuItem> menuItems, IBufermanHost bufermanHost)
        {
            menuItems.Add(bufermanHost.CreateMenuSeparatorItem());

            var openFileLocationMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuOpenFileLocation, (object sender, EventArgs args) =>
            {
                var arguments = $"/select, \"{this._filePath}\"";
                Process.Start("explorer.exe", arguments).WaitForInputIdle();
            });
            openFileLocationMenuItem.ShortCut = Shortcut.CtrlShiftF;

            menuItems.Add(openFileLocationMenuItem);
            //menuItems.Add(MenuItem to copy file path); // TODO (m)
            //menuItems.Add(MenuItem to copy folder's files); - as a part of plugin

            return menuItems;
        }
    }
}
