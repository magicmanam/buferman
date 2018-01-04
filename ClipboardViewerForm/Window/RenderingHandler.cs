using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ClipboardBufer;
using Logging;
using ClipboardViewerForm.ClipMenu;

namespace ClipboardViewerForm.Window
{
	class RenderingHandler : IRenderingHandler
    {
        private readonly Form _form;
        private readonly IClipboardBuferService _clipboardBuferService;
		private readonly IWindowHidingHandler _hidingHandler;
		private readonly IDictionary<IDataObject, Button> _buttonsMap;
		private readonly IEqualityComparer<IDataObject> _comparer;
        private readonly int _buttonWidth;
        private readonly Label _persistentClipsDivider;
        
        private const int BUTTON_HEIGHT = 25;

        public RenderingHandler(Form form, IClipboardBuferService clipboardBuferService, IEqualityComparer<IDataObject> comparer, IWindowHidingHandler hidingHandler)
        {
            this._form = form;
            this._clipboardBuferService = clipboardBuferService;
			this._comparer = comparer;
			this._hidingHandler = hidingHandler;
            this._buttonsMap = new Dictionary<IDataObject, Button>(clipboardBuferService.MaxBuferCount);
            this._buttonWidth = this._form.ClientRectangle.Width;
            this._persistentClipsDivider = new Label() { Text = string.Empty, BorderStyle = BorderStyle.FixedSingle, AutoSize = false, Height = 3, BackColor = Color.AliceBlue, Width = this._buttonWidth };
            this._form.Controls.Add(this._persistentClipsDivider);
        }

        public void Render()
        {
            var temporaryClips = this._clipboardBuferService.GetTemporaryClips().ToList();
            var persistentClips = this._clipboardBuferService.GetPersistentClips();
            this._RemoveOldButtons(temporaryClips.Union(persistentClips));

            this._DrawButtonsForBufers(temporaryClips, 0);
            this._persistentClipsDivider.Location = new Point(0, temporaryClips.Count * BUTTON_HEIGHT);

            if (persistentClips.Any())
            {
                this._DrawButtonsForBufers(persistentClips.ToList(), this._persistentClipsDivider.Location.Y + this._persistentClipsDivider.Height);
            }
        }

        private void _DrawButtonsForBufers(List<IDataObject> bufers, int startFromY)
        {
            int currentButtonIndex = bufers.Count - 1;
            int y = currentButtonIndex * BUTTON_HEIGHT + startFromY;

            foreach (var bufer in bufers)
            {
                if (bufer.GetFormats().Length == 0)
                {
                    //In this case we do not need to have Ctrl+Z available, so, undoable context is quite good here
                    this._clipboardBuferService.RemoveClip(bufer);
                    continue;
                }

                Button button;
                var equalObject = this._buttonsMap.Keys.FirstOrDefault(k => this._comparer.Equals(k, bufer));

                if (equalObject != null)
                {
                    button = this._buttonsMap[equalObject];
                }
                else
                {
                    button = new Button() { TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0), Width = this._buttonWidth };

                    this._buttonsMap.Add(bufer, button);
                    this._form.Controls.Add(button);

                    var buferSelectionHandler = new BuferSelectionHandler(this._form, bufer, this._hidingHandler);

                    new BuferHandlersWrapper(this._clipboardBuferService, this, bufer, button, this._form, new ClipMenuGenerator(this._clipboardBuferService, this, buferSelectionHandler, this._hidingHandler), buferSelectionHandler);
                }

                button.TabIndex = currentButtonIndex;
                button.Location = new Point(0, y);

                currentButtonIndex -= 1;
                y -= BUTTON_HEIGHT;
            }
        }

        private void _RemoveOldButtons(IEnumerable<IDataObject> bufers)
        {
			var deletedKeys = new List<IDataObject>();

            foreach (var key in this._buttonsMap.Keys.ToList())
            {
				var equalKey = bufers.FirstOrDefault(b => this._comparer.Equals(key, b));
                if (equalKey == null)
                {
                    this._form.Controls.Remove(this._buttonsMap[key]);
                    deletedKeys.Add(key);
                }
            }

            foreach (var key in deletedKeys)
            {
                this._buttonsMap.Remove(key);
            }
        }

		 public void OnKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Escape:
					this._hidingHandler.HideWindow();
					break;
				case Keys.Space:
					SendKeys.Send("~");
					break;
				case Keys.C:
					SendKeys.Send("{TAB}");
					SendKeys.Send("{TAB}");
					SendKeys.Send("{TAB}");
					break;
				case Keys.X:
					var lastBufer = this._clipboardBuferService.LastTemporaryClip;
					if(lastBufer != null)
					{
						var button = this._buttonsMap[lastBufer];
						button.Focus();
					}					
					break;
				case Keys.V:
					var firstBufer = this._clipboardBuferService.FirstClip;
					if (firstBufer != null)
					{
						var button = this._buttonsMap[firstBufer];
						button.Focus();
					}
					break;
			}
		}		
    }
}