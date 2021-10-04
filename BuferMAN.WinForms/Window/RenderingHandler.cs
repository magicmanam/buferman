using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Window;
using BuferMAN.Plugins.BuferPresentations;
using BuferMAN.BuferPresentations;
using BuferMAN.View;
using BuferMAN.Infrastructure.Settings;

namespace BuferMAN.WinForms.Window
{
    internal class RenderingHandler : IRenderingHandler
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IBuferHandlersBinder _buferHandlersBinder;
        private readonly IProgramSettingsGetter _settings;
        private readonly IList<IBuferPresentation> _clipPresentations = new List<IBuferPresentation>() { new SkypeBuferPresentation(), new FileContentsBuferPresentation() };

        private const int BUTTON_HEIGHT = 23;

        public RenderingHandler(IClipboardBuferService clipboardBuferService, IProgramSettingsGetter settings, IBuferHandlersBinder buferHandlersBinder)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._settings = settings;
            this._buferHandlersBinder = buferHandlersBinder;
        }

        public void Render(IBufermanHost bufermanHost)
        {
            var pinnedBufers = this._clipboardBuferService.GetPinnedBufers();
            var temporaryBufers = this._clipboardBuferService.GetTemporaryBufers().ToList();

            bufermanHost.SuspendLayoutLogic();

            if (this._clipboardBuferService.BufersCount > this._settings.MaxBufersCount)
            {
                temporaryBufers = temporaryBufers.Skip(this._clipboardBuferService.BufersCount - this._settings.MaxBufersCount).ToList();
            }// TODO (l) remove this after scrolling will be added

            var deletedBufers = new List<IBufer>();

            foreach (var bufer in bufermanHost.Bufers)
            {
                var equalKey = temporaryBufers
                    .Union(pinnedBufers)
                    .FirstOrDefault(b => b.ViewId == bufer.ViewModel.ViewId);

                if (equalKey == null)
                {
                    deletedBufers.Add(bufer);
                }
            }

            foreach (var bufer in deletedBufers)
            {
                bufermanHost.RemoveBufer(bufer);
            }

            if (temporaryBufers.Any())
            {
                this._DrawButtonsForBufers(bufermanHost, temporaryBufers, temporaryBufers.Count * BUTTON_HEIGHT - BUTTON_HEIGHT, temporaryBufers.Count - 1);
            }

            var pinnedBufersDividerY = temporaryBufers.Count * BUTTON_HEIGHT + 1;
            bufermanHost.SetPinnedBufersDividerY(pinnedBufersDividerY);

            if (pinnedBufers.Any())
            {
                this._DrawButtonsForBufers(
                    bufermanHost,
                    pinnedBufers.ToList(),
                    pinnedBufersDividerY + bufermanHost.PinnedBufersDividerHeight + 1 + pinnedBufers.Count() * BUTTON_HEIGHT - BUTTON_HEIGHT,
                    temporaryBufers.Count + pinnedBufers.Count() - 1,
                    true);
            }

            bufermanHost.ResumeLayoutLogic();
        }

        private void _DrawButtonsForBufers(IBufermanHost bufermanHost, List<BuferViewModel> bufers, int y, int currentButtonIndex,
            bool pinned = false)// TODO (l) remove this parameter: get from bufers collection, but be careful!!!
        {
            foreach (var buferViewModel in bufers)
            {
                Button button;
                var bufer = bufermanHost.Bufers.SingleOrDefault(b => b.ViewModel.ViewId == buferViewModel.ViewId);

                if (bufer == null)
                {
                    bufer = new Bufer()
                    {
                        BackColor = this._settings.BuferDefaultBackgroundColor,
                        ViewModel = buferViewModel,
                        Width = bufermanHost.InnerAreaWidth
                    };
                    this._buferHandlersBinder.Bind(bufer, bufermanHost);

                    this._TryApplyPresentation(bufer);
                    bufermanHost.AddBufer(bufer);
                }

                button = bufer.GetButton();

                var isCurrentBufer = buferViewModel.ViewId == bufermanHost.CurrentBuferViewId;
                for (var i = 0; i < button.ContextMenu.MenuItems.Count; i++)
                {// TODO (m) remove this shit
                    var menuItem = button.ContextMenu.MenuItems[i];

                    if (menuItem.Shortcut == Shortcut.CtrlC)
                    {
                        menuItem.Enabled = !isCurrentBufer;
                    }
                    else
                    {
                        for (var j = 0; j < menuItem.MenuItems.Count; j++)
                        {
                            var nestedMenuItem = menuItem.MenuItems[j];
                            if (nestedMenuItem.Shortcut == Shortcut.CtrlC)
                            {
                                nestedMenuItem.Enabled = !isCurrentBufer;
                            }
                        }
                    }
                }// TODO (l) maybe remove this menu item if bufer is current? I can do this if rerender context menu on every change in clipboard service

                var defaultBackColor = isCurrentBufer ?
                    (pinned ? this._settings.PinnedCurrentBuferBackColor : this._settings.CurrentBuferBackgroundColor) :
                    (pinned ? this._settings.PinnedBuferBackgroundColor : this._settings.BuferDefaultBackgroundColor);

                button.BackColor = defaultBackColor;
                bufer.ViewModel.DefaultBackColor = defaultBackColor;

                button.TabIndex = currentButtonIndex;
                button.Location = new Point(0, y);

                currentButtonIndex -= 1;
                y -= BUTTON_HEIGHT;
            }
        }

        private void _TryApplyPresentation(IBufer bufer)
        {
            foreach (var presentation in this._clipPresentations)
            {
                if (presentation.IsCompatibleWithBufer(bufer.ViewModel.Clip))
                {
                    presentation.ApplyToButton(bufer.GetButton());
                    return;
                }
            }
        }
    }
}