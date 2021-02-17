using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Window;
using BuferMAN.ContextMenu;
using BuferMAN.Plugins.BuferPresentations;
using BuferMAN.BuferPresentations;
using magicmanam.UndoRedo;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.View;

namespace BuferMAN.Form.Window
{
	class RenderingHandler : IRenderingHandler
    {
        private readonly BuferAMForm _form;
        private readonly IClipboardBuferService _clipboardBuferService;
		private readonly IEqualityComparer<IDataObject> _comparer;
        private readonly int _buttonWidth;
        private readonly Label _persistentClipsDivider;
        private readonly IClipboardWrapper _clipboardWrapper;
        private readonly IProgramSettings _settings;
        private readonly IFileStorage _fileStorage;
        private readonly IDictionary<IDataObject, Button> _removedButtons = new Dictionary<IDataObject, Button>();
        private readonly IList<IBuferPresentation> _clipPresentations = new List<IBuferPresentation>() { new SkypeBuferPresentation(), new FileContentsBuferPresentation() };
        private readonly Color FOCUSED_CLIP_BACK_COLOR = Color.LightSteelBlue;
        private readonly Color DEFAULT_CLIP_BACK_COLOR = Color.Silver;

        private const int BUTTON_HEIGHT = 23;

        public RenderingHandler(BuferAMForm form, IClipboardBuferService clipboardBuferService, IEqualityComparer<IDataObject> comparer, IClipboardWrapper clipboardWrapper, IProgramSettings settings, IFileStorage fileStorage)
        {
            this._form = form;
            this._clipboardBuferService = clipboardBuferService;
			this._comparer = comparer;
            this._buttonWidth = this._form.ClientRectangle.Width;
            this._persistentClipsDivider = new Label() { Text = string.Empty, BorderStyle = BorderStyle.FixedSingle, AutoSize = false, Height = 3, BackColor = Color.AliceBlue, Width = this._buttonWidth };
            this._form.Controls.Add(this._persistentClipsDivider);
            this._persistentClipsDivider.BringToFront();
            this._clipboardWrapper = clipboardWrapper;
            this._settings = settings;
            this._fileStorage = fileStorage;
        }

        public void Render()
        {
            var temporaryClips = this._clipboardBuferService.GetTemporaryClips().ToList();
            var persistentClips = this._clipboardBuferService.GetPersistentClips();

            var extraTemporaryClipsCount = Math.Max(this._clipboardBuferService.ClipsCount - BuferAMForm.MAX_BUFERS_COUNT, 0);
            temporaryClips = temporaryClips.Skip(extraTemporaryClipsCount).ToList();

            this._RemoveOldButtons(temporaryClips.Union(persistentClips));

            if (temporaryClips.Any())
            {
                this._DrawButtonsForBufers(temporaryClips, temporaryClips.Count * BUTTON_HEIGHT - BUTTON_HEIGHT, temporaryClips.Count - 1);
            }

            this._persistentClipsDivider.Location = new Point(0, temporaryClips.Count * BUTTON_HEIGHT + 1);

            if (persistentClips.Any())
            {
                this._DrawButtonsForBufers(persistentClips.ToList(), this._persistentClipsDivider.Location.Y + this._persistentClipsDivider.Height + 1 + persistentClips.Count() * BUTTON_HEIGHT - BUTTON_HEIGHT, temporaryClips.Count + persistentClips.Count() - 1, true);
            }
        }

        private void _DrawButtonsForBufers(List<IDataObject> bufers, int y, int currentButtonIndex, bool persistent = false)
        {
            foreach (var bufer in bufers)
            {
                if (bufer.GetFormats().Length == 0)
                {
                    this._RemoveBuferWithoutTrackingInUndoableContext(bufer);
                }
                else
                {
                    Button button;
                    var equalObject = this._form.ButtonsMap.Keys.FirstOrDefault(k => this._comparer.Equals(k, bufer));

                    if (equalObject != null)
                    {
                        button = this._form.ButtonsMap[equalObject];
                    }
                    else
                    {
                        var equalObjectFromDeleted = this._removedButtons.Keys.FirstOrDefault(k => this._comparer.Equals(k, bufer));

                        if (equalObjectFromDeleted != null)
                        {
                            button = this._removedButtons[equalObjectFromDeleted];
                            this._removedButtons.Remove(equalObjectFromDeleted);
                        }
                        else
                        {
                            button = new Button() { TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0), Width = this._buttonWidth, BackColor = DEFAULT_CLIP_BACK_COLOR };
                            button.GotFocus += Clip_GotFocus;
                            button.LostFocus += Clip_LostFocus;

                            var buferSelectionHandler = new BuferSelectionHandler(this._form, bufer, this._clipboardWrapper);

                            new BuferHandlersWrapper(this._clipboardBuferService, new BuferViewModel { Clip = bufer, Persistent = persistent }, button, this._form, new ClipMenuGenerator(this._clipboardBuferService, buferSelectionHandler, this._settings, this._clipboardWrapper), buferSelectionHandler, this._fileStorage);

                            this._TryApplyPresentation(bufer, button);
                        }
                        this._form.ButtonsMap.Add(bufer, button);
                        this._form.Controls.Add(button);
                        button.BringToFront();
                    }

                    if (persistent)
                    {
                        foreach (var item in button.ContextMenu.MenuItems)
                        {
                            var persistentMenuItem = item as MakePersistentMenuItem;
                            if (persistentMenuItem != null)
                            {
                                persistentMenuItem.Enabled = false;
                            }
                        }
                    }

                    button.BackColor = persistent ? Color.LightSlateGray : DEFAULT_CLIP_BACK_COLOR;
                    (button.Tag as ButtonData).DefaultBackColor = button.BackColor;

                    button.TabIndex = currentButtonIndex;
                    button.Location = new Point(0, y);
                }

                currentButtonIndex -= 1;
                y -= BUTTON_HEIGHT;
            }
        }
                
        private void Clip_GotFocus(object sender, System.EventArgs e)
        {
            var button = sender as Button;
            button.BackColor = FOCUSED_CLIP_BACK_COLOR;
        }

        private void Clip_LostFocus(object sender, System.EventArgs e)
        {
            var button = sender as Button;
            button.BackColor = (button.Tag as ButtonData).DefaultBackColor;
        }

        private void _RemoveBuferWithoutTrackingInUndoableContext(IDataObject bufer)
        {
            using (var action = UndoableContext<ClipboardBuferServiceState>.Current.StartAction())
            {
                this._clipboardBuferService.RemoveClip(bufer);
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

        private void _RemoveOldButtons(IEnumerable<IDataObject> bufers)
        {
			var deletedKeys = new List<IDataObject>();

            foreach (var key in this._form.ButtonsMap.Keys.ToList())
            {
				var equalKey = bufers.FirstOrDefault(b => this._comparer.Equals(key, b));
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