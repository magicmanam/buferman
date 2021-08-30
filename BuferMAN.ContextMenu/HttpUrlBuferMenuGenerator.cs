using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class HttpUrlBuferMenuGenerator : IBuferTypeMenuGenerator
    {
        private readonly string _url;
        private readonly IProgramSettingsGetter _settingsGetter;
        private readonly IProgramSettingsSetter _settingsSetter;

        public HttpUrlBuferMenuGenerator(string url, IProgramSettingsGetter settingsGetter, IProgramSettingsSetter settingsSetter)
        {
            this._url = url;

            this._settingsGetter = settingsGetter;
            this._settingsSetter = settingsSetter;
        }

        public IList<BufermanMenuItem> Generate(IList<BufermanMenuItem> menuItems, IBufermanHost bufermanHost)
        {
            var httpUrlBuferExplanationCounter = this._settingsGetter.HttpUrlBuferExplanationCounter;

            if (httpUrlBuferExplanationCounter < int.MaxValue / 8)
            {
                if (this._IsPowerOfTwo(httpUrlBuferExplanationCounter))
                {
                    bufermanHost.NotificationEmitter.ShowInfoNotification(Resource.HttpUrlBuferExplanation, 2500);
                    httpUrlBuferExplanationCounter *= 4;
                }

                this._settingsSetter.HttpUrlBuferExplanationCounter = httpUrlBuferExplanationCounter - 1;
            }

            menuItems.Add(bufermanHost.CreateMenuSeparatorItem());

            var openInBrowserMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuOpenInBrowser, (object sender, EventArgs e) =>
                                    Process.Start(this._url));
            openInBrowserMenuItem.ShortCut = Shortcut.CtrlB;
            menuItems.Add(openInBrowserMenuItem);

            return menuItems;
        }

        private bool _IsPowerOfTwo(int number)
        {
            return (number & (number - 1)) == 0;
        }
    }
}
