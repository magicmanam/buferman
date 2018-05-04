﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Window;
using BuferMAN.ContextMenu;
using BuferMAN.Plugins.BuferPresentations;
using BuferMAN.BuferPresentations;

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
        private readonly IList<IBuferPresentation> _clipPresentations = new List<IBuferPresentation>() { new SkypeBuferPresentation(), new FileContentsBuferPresentation() };

        private const int BUTTON_HEIGHT = 23;

        public RenderingHandler(BuferAMForm form, IClipboardBuferService clipboardBuferService, IEqualityComparer<IDataObject> comparer, IClipboardWrapper clipboardWrapper, IProgramSettings settings)
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
        }

        public void Render()
        {
            var temporaryClips = this._clipboardBuferService.GetTemporaryClips().ToList();
            var persistentClips = this._clipboardBuferService.GetPersistentClips();
            this._RemoveOldButtons(temporaryClips.Union(persistentClips));

            if (temporaryClips.Any())
            {
                this._DrawButtonsForBufers(temporaryClips, temporaryClips.Count * BUTTON_HEIGHT - BUTTON_HEIGHT, temporaryClips.Count - 1);
            }

            this._persistentClipsDivider.Location = new Point(0, temporaryClips.Count * BUTTON_HEIGHT + 1);

            if (persistentClips.Any())
            {
                this._DrawButtonsForBufers(persistentClips.ToList(), this._persistentClipsDivider.Location.Y + this._persistentClipsDivider.Height + 1 + persistentClips.Count() * BUTTON_HEIGHT - BUTTON_HEIGHT, temporaryClips.Count + persistentClips.Count() - 1);
            }
        }

        private void _DrawButtonsForBufers(List<IDataObject> bufers, int y, int currentButtonIndex)
        {

            foreach (var bufer in bufers)
            {
                if (bufer.GetFormats().Length == 0)
                {
                    //In this case we do not need to have Ctrl+Z available, so, undoable context is quite good here
                    this._clipboardBuferService.RemoveClip(bufer);
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
                        button = new Button() { TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0), Width = this._buttonWidth };

                        this._form.ButtonsMap.Add(bufer, button);
                        this._form.Controls.Add(button);
                        button.BringToFront();
                        var buferSelectionHandler = new BuferSelectionHandler(this._form, bufer, this._clipboardWrapper);

                        new BuferHandlersWrapper(this._clipboardBuferService, bufer, button, this._form, new ClipMenuGenerator(this._clipboardBuferService, buferSelectionHandler, this._settings, this._clipboardWrapper), buferSelectionHandler);

                        this._TryApplyPresentation(bufer, button);
                    }

                    button.TabIndex = currentButtonIndex;
                    button.Location = new Point(0, y);
                }

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

        private void _RemoveOldButtons(IEnumerable<IDataObject> bufers)
        {
			var deletedKeys = new List<IDataObject>();

            foreach (var key in this._form.ButtonsMap.Keys.ToList())
            {
				var equalKey = bufers.FirstOrDefault(b => this._comparer.Equals(key, b));
                if (equalKey == null)
                {
                    this._form.Controls.Remove(this._form.ButtonsMap[key]);
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