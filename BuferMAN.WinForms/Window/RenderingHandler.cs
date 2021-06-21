using System;
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
        private BufermanWindow _form;// TODO (m) must be IBuferMANHost
        private readonly IClipboardBuferService _clipboardBuferService;
        private int _buttonWidth;
        private Label _pinnedClipsDivider;// TODO (m) replace with Split Container (along with scrolling bufers feature and pinned area)
        private readonly IBuferHandlersBinder _buferHandlersBinder;
        private readonly IProgramSettingsGetter _settings;
        private readonly IList<IBuferPresentation> _clipPresentations = new List<IBuferPresentation>() { new SkypeBuferPresentation(), new FileContentsBuferPresentation() };
        private BuferViewModel _currentBufer;

        private const int BUTTON_HEIGHT = 23;

        public RenderingHandler(IClipboardBuferService clipboardBuferService, IProgramSettingsGetter settings, IBuferHandlersBinder buferHandlersBinder)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._settings = settings;
            this._buferHandlersBinder = buferHandlersBinder;
        }

        public void SetForm(Form form)
        {
            var f = form as BufermanWindow;// TODO (l) : remove this assignment

            this._form = f;
            this._buttonWidth = this._form.ClientRectangle.Width;
            this._pinnedClipsDivider = new Label() { Text = string.Empty, BorderStyle = BorderStyle.FixedSingle, AutoSize = false, Height = 3, BackColor = Color.AliceBlue, Width = this._buttonWidth };
            this._form.Controls.Add(this._pinnedClipsDivider);
            this._pinnedClipsDivider.BringToFront();
        }

        public void SetCurrentBufer(BuferViewModel bufer)
        {
            this._currentBufer = bufer;
        }

        public void Render(IBufermanHost bufermanHost)
        {
            var pinnedBufers = this._clipboardBuferService.GetPinnedBufers();
            var temporaryBufers = this._clipboardBuferService.GetTemporaryBufers().ToList();

            this._form.SuspendLayout();

            if (this._clipboardBuferService.BufersCount > this._settings.MaxBufersCount)
            {
                temporaryBufers = temporaryBufers.Skip(this._clipboardBuferService.BufersCount - this._settings.MaxBufersCount).ToList();
            }// TODO (l) remove this after scrolling will be added

            this._RemoveOldButtons(temporaryBufers.Union(pinnedBufers));

            if (temporaryBufers.Any())
            {
                this._DrawButtonsForBufers(bufermanHost, temporaryBufers, temporaryBufers.Count * BUTTON_HEIGHT - BUTTON_HEIGHT, temporaryBufers.Count - 1);
            }

            this._pinnedClipsDivider.Location = new Point(0, temporaryBufers.Count * BUTTON_HEIGHT + 1);

            if (pinnedBufers.Any())
            {
                this._DrawButtonsForBufers(bufermanHost, pinnedBufers.ToList(), this._pinnedClipsDivider.Location.Y + this._pinnedClipsDivider.Height + 1 + pinnedBufers.Count() * BUTTON_HEIGHT - BUTTON_HEIGHT, temporaryBufers.Count + pinnedBufers.Count() - 1, true);
            }

            this._form.ResumeLayout(false);
        }

        private void _DrawButtonsForBufers(IBufermanHost bufermanHost, List<BuferViewModel> bufers, int y, int currentButtonIndex,
            bool pinned = false)// TODO (l) remove this parameter: get from bufers collection, but be careful!!!
        {
            foreach (var buferViewModel in bufers)
            {
                Button button;
                var equalObject = this._form.ButtonsMap.ContainsKey(buferViewModel.ViewId);

                if (equalObject)
                {
                    button = this._form.ButtonsMap[buferViewModel.ViewId];
                }
                else
                {
                    var bufer = new Bufer()
                    {
                        Width = this._buttonWidth,
                        BackColor = this._settings.BuferDefaultBackgroundColor,
                        ViewModel = buferViewModel
                    };
                    this._buferHandlersBinder.Bind(bufer, bufermanHost);

                    button = bufer.GetButton();
                    button.Tag = bufer;// TODO (m) remove Tag usage!
                    this._TryApplyPresentation(buferViewModel.Clip, button);

                    this._form.ButtonsMap.Add(buferViewModel.ViewId, button);
                    this._form.Controls.Add(button);
                    button.BringToFront();
                }

                for (var i = 0; i < button.ContextMenu.MenuItems.Count; i++)
                {// TODO (m) remove this shit
                    var menuItem = button.ContextMenu.MenuItems[i];

                    if (menuItem.Shortcut == Shortcut.CtrlC)
                    {
                        menuItem.Enabled = buferViewModel.ViewId != this._currentBufer.ViewId;
                    }
                    else
                    {
                        for (var j = 0; j < menuItem.MenuItems.Count; j++)
                        {
                            var nestedMenuItem = menuItem.MenuItems[j];
                            if (nestedMenuItem.Shortcut == Shortcut.CtrlC)
                            {
                                nestedMenuItem.Enabled = buferViewModel.ViewId != this._currentBufer.ViewId;
                            }
                        }
                    }
                }// TODO (l) maybe remove this menu item if bufer is current? I can do this if rerender context menu on every change in clipboard service

                var defaultBackColor = buferViewModel.ViewId == this._currentBufer.ViewId ?
                    (pinned ? this._settings.PinnedCurrentBuferBackColor : this._settings.CurrentBuferBackgroundColor) :
                    (pinned ? this._settings.PinnedBuferBackgroundColor : this._settings.BuferDefaultBackgroundColor);

                button.BackColor = defaultBackColor;
                (button.Tag as IBufer).ViewModel.DefaultBackColor = defaultBackColor;

                button.TabIndex = currentButtonIndex;
                button.Location = new Point(0, y);

                currentButtonIndex -= 1;
                y -= BUTTON_HEIGHT;
            }
        }

        private void _TryApplyPresentation(IDataObject dataObject, Button button)
        {
            foreach (var presentation in this._clipPresentations)
            {
                if (presentation.IsCompatibleWithBufer(dataObject))
                {
                    presentation.ApplyToButton(button);
                    return;
                }
            }
        }

        private void _RemoveOldButtons(IEnumerable<BuferViewModel> bufers)
        {
			var deletedKeys = new List<Guid>();

            foreach (var key in this._form.ButtonsMap.Keys.ToList())
            {
                var equalKey = bufers.FirstOrDefault(b => b.ViewId == key);
                if (equalKey == null)
                {
                    var button = this._form.ButtonsMap[key];
                    this._form.Controls.Remove(button);
                    deletedKeys.Add(key);
                }
            }

            foreach (var key in deletedKeys)
            {
                this._form.ButtonsMap.Remove(key);
            }
        }
    }
}