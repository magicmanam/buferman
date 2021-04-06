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
using magicmanam.UndoRedo;
using BuferMAN.View;

namespace BuferMAN.Form.Window
{
	public class RenderingHandler : IRenderingHandler
    {
        private BuferAMForm _form;// TODO (m) must be IBuferMANHost
        private readonly IClipboardBuferService _clipboardBuferService;
        private int _buttonWidth;
        private Label _pinnedClipsDivider;
        private readonly IBuferHandlersBinder _buferHandlersBinder;
        private readonly IProgramSettings _settings;
        private readonly IDictionary<Guid, Button> _removedButtons = new Dictionary<Guid, Button>();
        private readonly IList<IBuferPresentation> _clipPresentations = new List<IBuferPresentation>() { new SkypeBuferPresentation(), new FileContentsBuferPresentation() };
        private readonly Color FOCUSED_BUFER_BACK_COLOR = Color.LightSteelBlue;
        private readonly Color DEFAULT_BUFER_BACK_COLOR = Color.Silver;
        private readonly Color PINNED_BUFER_BACK_COLOR = Color.LightSlateGray;
        private readonly Color PINNED_CURRENT_BUFER_BACK_COLOR = Color.LawnGreen;
        private readonly Color DEFAULT_CURRENT_BUFER_BACK_COLOR = Color.LightGreen;
        private BuferViewModel _currentBufer;

        private const int BUTTON_HEIGHT = 23;

        public RenderingHandler(IClipboardBuferService clipboardBuferService, IProgramSettings settings, IBuferHandlersBinder buferHandlersBinder)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._settings = settings;
            this._buferHandlersBinder = buferHandlersBinder;
        }

        public void SetForm(System.Windows.Forms.Form form)
        {
            BuferAMForm f = form as BuferAMForm;// TODO (l) : remove this assignment

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

            var emptyClipFound = false;
            foreach(var bufer in pinnedBufers)
            {
                if (bufer.Clip.GetFormats().Length == 0)
                {
                    emptyClipFound = true;
                    this._RemoveClipWithoutTrackingInUndoableContext(bufer);
                }
            }

            if (emptyClipFound)
            {
                pinnedBufers = this._clipboardBuferService.GetPinnedBufers();
            }

            var temporaryBufers = this._clipboardBuferService.GetTemporaryClips().ToList();

            do
            {
                emptyClipFound = false;
                var extraTemporaryClipsCount = Math.Max(this._clipboardBuferService.BufersCount - this._settings.MaxBufersCount, 0);
                temporaryBufers = temporaryBufers.Skip(extraTemporaryClipsCount).ToList();

                foreach (var bufer in temporaryBufers)
                {
                    if (bufer.Clip.GetFormats().Length == 0)
                    {
                        emptyClipFound = true;
                        this._RemoveClipWithoutTrackingInUndoableContext(bufer);
                    }
                }

                if (emptyClipFound)
                {
                    temporaryBufers = this._clipboardBuferService.GetTemporaryClips().ToList();
                }
            } while (emptyClipFound);

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
        }

        private void _DrawButtonsForBufers(IBufermanHost bufermanHost, List<BuferViewModel> bufers, int y, int currentButtonIndex,
            bool pinned = false)// TODO (l) remove this parameter: get from bufers collection, but be careful!!!
        {
            foreach (var bufer in bufers)
            {
                Button button;
                var equalObject = this._form.ButtonsMap.ContainsKey(bufer.ViewId);

                if (equalObject)
                {
                    button = this._form.ButtonsMap[bufer.ViewId];
                }
                else
                {
                    var equalObjectFromDeleted = this._removedButtons.ContainsKey(bufer.ViewId);// TODO (s) now this property (_removedButtons) is not needed - this optimization does not make sense

                    if (equalObjectFromDeleted)
                    {
                        button = this._removedButtons[bufer.ViewId];
                        this._removedButtons.Remove(bufer.ViewId);
                    }
                    else
                    {
                        button = new Button() { TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0), Width = this._buttonWidth, BackColor = DEFAULT_BUFER_BACK_COLOR };
                        button.GotFocus += Clip_GotFocus;
                        button.LostFocus += Clip_LostFocus;

                        var buf = new Bufer();
                        this._buferHandlersBinder.Bind(bufer, button, buf, bufermanHost);

                        this._TryApplyPresentation(bufer.Clip, button);
                    }
                    this._form.ButtonsMap.Add(bufer.ViewId, button);
                    this._form.Controls.Add(button);
                    button.BringToFront();
                }

                for (var i = 0; i < button.ContextMenu.MenuItems.Count; i++)
                {
                    var menuItem = button.ContextMenu.MenuItems[i];
                    if (menuItem.Shortcut == Shortcut.CtrlS)// TODO (m) remove this condition or even the whole block - via Bufer class or handlers binder
                    {
                        menuItem.Enabled = !pinned;
                    }

                    for (var j = 0; j < menuItem.MenuItems.Count; j++)
                    {
                        var nestedMenuItem = menuItem.MenuItems[j];
                        if (nestedMenuItem.Shortcut == Shortcut.CtrlC)
                        {
                            nestedMenuItem.Enabled = bufer.ViewId != this._currentBufer.ViewId;
                        }
                    }
                }

                var defaultBackColor = bufer.ViewId == this._currentBufer.ViewId ?
                    (pinned ? PINNED_CURRENT_BUFER_BACK_COLOR : DEFAULT_CURRENT_BUFER_BACK_COLOR) :
                    (pinned ? PINNED_BUFER_BACK_COLOR : DEFAULT_BUFER_BACK_COLOR);

                button.BackColor = defaultBackColor;
                (button.Tag as BuferViewModel).DefaultBackColor = defaultBackColor;

                button.TabIndex = currentButtonIndex;
                button.Location = new Point(0, y);

                currentButtonIndex -= 1;
                y -= BUTTON_HEIGHT;
            }
        }
                
        private void Clip_GotFocus(object sender, EventArgs e)
        {
            var button = sender as Button;
            button.BackColor = FOCUSED_BUFER_BACK_COLOR;
        }

        private void Clip_LostFocus(object sender, EventArgs e)
        {
            var button = sender as Button;
            button.BackColor = (button.Tag as BuferViewModel).DefaultBackColor;
        }

        private void _RemoveClipWithoutTrackingInUndoableContext(BuferViewModel bufer)
        {
            using (var action = UndoableContext<ApplicationStateSnapshot>.Current.StartAction())
            {
                this._clipboardBuferService.RemoveBufer(bufer.ViewId);
                action.Cancel();
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

                    if (this._removedButtons.ContainsKey(key))
                    {
                        this._removedButtons.Remove(key);
                    }

                    this._removedButtons.Add(key, button);
                }
            }

            foreach (var key in deletedKeys)
            {
                this._form.ButtonsMap.Remove(key);
            }
        }
    }
}