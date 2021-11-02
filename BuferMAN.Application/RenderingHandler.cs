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
using BuferMAN.Infrastructure.Menu;

namespace BuferMAN.Application
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

        public void Render(IBufermanHost bufermanHost, IEnumerable<BuferViewModel> temporaryBuferViewModels, IEnumerable<BuferViewModel> pinnedBuferViewModels)
        {
            if (this._clipboardBuferService.BufersCount > this._settings.MaxBufersCount)
            {
                temporaryBuferViewModels = temporaryBuferViewModels.Skip(this._clipboardBuferService.BufersCount - this._settings.MaxBufersCount).ToList();
            }// TODO (l) remove this after scrolling will be added

            var deletedBufers = new List<IBufer>();

            foreach (var bufer in bufermanHost.Bufers)
            {
                var equalKey = temporaryBuferViewModels
                    .Union(pinnedBuferViewModels)
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

            if (temporaryBuferViewModels.Any())
            {
                this._DrawBufers(bufermanHost, temporaryBuferViewModels.ToList(), temporaryBuferViewModels.Count() * BUTTON_HEIGHT - BUTTON_HEIGHT, temporaryBuferViewModels.Count() - 1);
            }

            var pinnedBufersDividerY = temporaryBuferViewModels.Count() * BUTTON_HEIGHT + 1;
            bufermanHost.SetPinnedBufersDividerY(pinnedBufersDividerY);

            if (pinnedBuferViewModels.Any())
            {
                this._DrawBufers(
                    bufermanHost,
                    pinnedBuferViewModels.ToList(),
                    pinnedBufersDividerY + bufermanHost.PinnedBufersDividerHeight + 1 + pinnedBuferViewModels.Count() * BUTTON_HEIGHT - BUTTON_HEIGHT,
                    temporaryBuferViewModels.Count() + pinnedBuferViewModels.Count() - 1,
                    true);
            }
        }

        private void _DrawBufers(IBufermanHost bufermanHost, List<BuferViewModel> bufers, int y, int currentButtonIndex,
            bool pinned = false)// TODO (l) remove this parameter: get from bufers collection, but be careful!!!
        {
            foreach (var buferViewModel in bufers)
            {
                var bufer = bufermanHost.Bufers.SingleOrDefault(b => b.ViewModel.ViewId == buferViewModel.ViewId);

                if (bufer == null)
                {
                    bufer = bufermanHost.CreateBufer();
                    bufer.BackColor = this._settings.BuferDefaultBackgroundColor;
                    bufer.ViewModel = buferViewModel;
                    bufer.Width = bufermanHost.InnerAreaWidth;

                    this._buferHandlersBinder.Bind(bufer, bufermanHost);

                    this._TryApplyPresentation(bufer);
                    bufermanHost.AddBufer(bufer);
                }

                var isCurrentBufer = buferViewModel.ViewId == bufermanHost.CurrentBuferViewId;
                this._UpdateCtrlCMenuItem(bufer.ContextMenu, isCurrentBufer);

                var defaultBackColor = isCurrentBufer ?
                    (pinned ? this._settings.PinnedCurrentBuferBackColor : this._settings.CurrentBuferBackgroundColor) :
                    (pinned ? this._settings.PinnedBuferBackgroundColor : this._settings.BuferDefaultBackgroundColor);

                bufer.BackColor = defaultBackColor;
                bufer.ViewModel.DefaultBackColor = defaultBackColor;

                bufer.TabIndex = currentButtonIndex;
                bufer.Location = new Point(0, y);

                currentButtonIndex -= 1;
                y -= BUTTON_HEIGHT;
            }
        }

        private void _UpdateCtrlCMenuItem(IEnumerable<BufermanMenuItem> menuItems, bool isCurrentBufer)
        {
            foreach (var menuItem in menuItems)
            {
                if (menuItem.ShortCut == Shortcut.CtrlC)
                {
                    menuItem.Enabled = !isCurrentBufer;
                    return;
                }
                else
                {
                    this._UpdateCtrlCMenuItem(menuItem.Children, isCurrentBufer);
                }
            }
        }

        private void _TryApplyPresentation(IBufer bufer)
        {
            foreach (var presentation in this._clipPresentations)
            {
                if (presentation.IsCompatibleWithBufer(bufer.ViewModel.Clip))
                {
                    presentation.ApplyToBufer(bufer);
                    return;
                }
            }
        }
    }
}