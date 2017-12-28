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
        
        private const int BUTTON_HEIGHT = 25;

        public RenderingHandler(Form form, IClipboardBuferService clipboardBuferService, IEqualityComparer<IDataObject> comparer, IWindowHidingHandler hidingHandler)
        {
            this._form = form;
            this._clipboardBuferService = clipboardBuferService;
			this._comparer = comparer;
			this._hidingHandler = hidingHandler;
            this._buttonsMap = new Dictionary<IDataObject, Button>(clipboardBuferService.MaxBuferCount);
            this._buttonWidth = this._form.ClientRectangle.Width;
        }

        public void Render()
        {
            var bufers = _clipboardBuferService.GetClips(true).ToList();

            this.RemoveOldButtons(bufers);

            this.DrawButtonsForBufers(bufers);
        }

        private void DrawButtonsForBufers(List<IDataObject> bufers)
        {
            int currentButtonIndex = this._clipboardBuferService.ClipsCount - 1;
            int y = currentButtonIndex * BUTTON_HEIGHT;

            foreach (var bufer in bufers)
            {
                if (bufer.GetFormats().Length == 0)//Debug
                {
                    MessageBox.Show("On Rendering handler, bufer.GetFormats().Length = 0. Delete some button to avoid this message in the future and find out why it occured.");
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

                    new BuferHandlersWrapper(this._clipboardBuferService, this, bufer, button, this._form, new ClipMenuGenerator(this._clipboardBuferService, this));
                }

                button.TabIndex = currentButtonIndex;
                button.Location = new Point(0, y);

                currentButtonIndex -= 1;
                y -= BUTTON_HEIGHT;
            }
        }

        private void RemoveOldButtons(IEnumerable<IDataObject> bufers)
        {
			Logger.Write("Rendering Handler: Remove Old Buttons");
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