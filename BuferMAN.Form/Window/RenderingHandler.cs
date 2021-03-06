﻿using System;
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
	public class RenderingHandler : IRenderingHandler
    {
        private readonly BuferAMForm _form;
        private readonly IClipboardBuferService _clipboardBuferService;
		private readonly IEqualityComparer<IDataObject> _comparer;
        private readonly int _buttonWidth;
        private readonly Label _pinnedClipsDivider;
        private readonly IClipboardWrapper _clipboardWrapper;
        private readonly IProgramSettings _settings;
        private readonly IFileStorage _fileStorage;
        private readonly IDictionary<Guid, Button> _removedButtons = new Dictionary<Guid, Button>();
        private readonly IList<IBuferPresentation> _clipPresentations = new List<IBuferPresentation>() { new SkypeBuferPresentation(), new FileContentsBuferPresentation() };
        private readonly Color FOCUSED_BUFER_BACK_COLOR = Color.LightSteelBlue;
        private readonly Color DEFAULT_BUFER_BACK_COLOR = Color.Silver;
        private readonly Color PINNED_BUFER_BACK_COLOR = Color.LightSlateGray;

        private const int BUTTON_HEIGHT = 23;

        public RenderingHandler(BuferAMForm form, IClipboardBuferService clipboardBuferService, IEqualityComparer<IDataObject> comparer, IClipboardWrapper clipboardWrapper, IProgramSettings settings, IFileStorage fileStorage)
        {
            this._form = form;
            this._clipboardBuferService = clipboardBuferService;
			this._comparer = comparer;
            this._buttonWidth = this._form.ClientRectangle.Width;
            this._pinnedClipsDivider = new Label() { Text = string.Empty, BorderStyle = BorderStyle.FixedSingle, AutoSize = false, Height = 3, BackColor = Color.AliceBlue, Width = this._buttonWidth };
            this._form.Controls.Add(this._pinnedClipsDivider);
            this._pinnedClipsDivider.BringToFront();
            this._clipboardWrapper = clipboardWrapper;
            this._settings = settings;
            this._fileStorage = fileStorage;
        }

        public void Render()
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
                this._DrawButtonsForBufers(temporaryBufers, temporaryBufers.Count * BUTTON_HEIGHT - BUTTON_HEIGHT, temporaryBufers.Count - 1);
            }

            this._pinnedClipsDivider.Location = new Point(0, temporaryBufers.Count * BUTTON_HEIGHT + 1);

            if (pinnedBufers.Any())
            {
                this._DrawButtonsForBufers(pinnedBufers.ToList(), this._pinnedClipsDivider.Location.Y + this._pinnedClipsDivider.Height + 1 + pinnedBufers.Count() * BUTTON_HEIGHT - BUTTON_HEIGHT, temporaryBufers.Count + pinnedBufers.Count() - 1, true);
            }
        }

        private void _DrawButtonsForBufers(List<BuferViewModel> bufers, int y, int currentButtonIndex, bool persistent = false)
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
                    var equalObjectFromDeleted = this._removedButtons.ContainsKey(bufer.ViewId);

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

                        var buferSelectionHandler = new BuferSelectionHandler(bufer.Clip, this._clipboardWrapper);

                        new BuferHandlersWrapper(this._clipboardBuferService, bufer, button, new ClipMenuGenerator(this._clipboardBuferService, buferSelectionHandler, this._settings, this._clipboardWrapper), buferSelectionHandler, this._fileStorage);

                        this._TryApplyPresentation(bufer.Clip, button);
                    }
                    this._form.ButtonsMap.Add(bufer.ViewId, button);
                    this._form.Controls.Add(button);
                    button.BringToFront();
                }

                if (persistent)
                {
                    foreach (var item in button.ContextMenu.MenuItems)
                    {
                        var persistentMenuItem = item as MakePinnedMenuItem;
                        if (persistentMenuItem != null)
                        {
                            persistentMenuItem.Enabled = false;
                        }
                    }
                }

                button.BackColor = persistent ? PINNED_BUFER_BACK_COLOR : DEFAULT_BUFER_BACK_COLOR;
                (button.Tag as BuferViewModel).DefaultBackColor = button.BackColor;

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