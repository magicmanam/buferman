using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ClipboardViewer.Window
{
	class RenderingHandler : IRenderingHandler
    {
        private readonly Form _form;
        private readonly IClipboardBuferService _clipboardBuferService;
		private readonly IWindowHidingHandler _hidingHandler;
		private readonly IDictionary<IDataObject, Button> _buttonsMap = new Dictionary<IDataObject, Button>(30);
		private readonly IEqualityComparer<IDataObject> _comparer;
        
        private const int BUTTON_HEIGHT = 25;

        public RenderingHandler(Form form, IClipboardBuferService clipboardBuferService, IEqualityComparer<IDataObject> comparer, IWindowHidingHandler hidingHandler)
        {
            this._form = form;
            this._clipboardBuferService = clipboardBuferService;
			this._comparer = comparer;
			this._hidingHandler = hidingHandler;
        }

        public void Render()
        {
			Logger.Logger.Current.Write("On render");
            var bufers = _clipboardBuferService.GetClips(true).ToArray();
            
            int y = bufers.Length * BUTTON_HEIGHT;
            int width = _form.ClientRectangle.Width;
            
            this.RemoveOldButtons(bufers);
            int buttonIndex = bufers.Length;

            foreach (var bufer in bufers)
            {
                Button button;
				var equalObject = this._buttonsMap.Keys.FirstOrDefault(k => this._comparer.Equals(k, bufer));
                if (equalObject != null)
                {
                    button = this._buttonsMap[equalObject];                    
                }
                else
                {
                    var buferString = bufer.GetData("System.String") as string;//Only this format to support Unicode
                    button = new Button() { TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0) };
					var isChangeTextAvailable = true;
					string buferTitle = null;
                    if (buferString == null)
                    {
                        var files = bufer.GetData("FileDrop") as string[];
                        if (files != null && files.Length > 0)
                        {
							isChangeTextAvailable = false;
							var firstFileName = files.GetValue(0) as string;
                            
                            if (files.Length == 1)
                            {
                                buferTitle = $"<< File >>:";
                            }
                            else
                            {
                                buferTitle = $"<< Files ({files.Length}) >>:";
                            }

							var folder = Path.GetDirectoryName(files.First());
							buferString += folder + ": " + Environment.NewLine + Environment.NewLine;
							buferString += string.Join(Environment.NewLine, files.Select(f => Path.GetFileName(f)).ToList());

                            button.BackColor = Color.Brown;
                        }
                    }

                    if (buferString == null)
                    {
                        var isBitmap = bufer.GetFormats().Contains("Bitmap");
                        if (isBitmap)
                        {
							isChangeTextAvailable = false;
							buferString = "<< Image >>";
                            button.Font = new Font(button.Font, FontStyle.Italic | FontStyle.Bold);
                        }
                    }

                    button.Text = (buferTitle ?? buferString).Trim();
                    
                    this._buttonsMap.Add(bufer, button);
                    this._form.Controls.Add(button);

                    var contextMenu = new ContextMenu();
                    var formats = bufer.GetFormats();
                    var formatsMenu = new MenuItem($"Formats ({formats.Length})");
                    formatsMenu.Shortcut = Shortcut.AltDownArrow;
                    foreach (var format in bufer.GetFormats())
                    {
                        var particularFormatMenu = new MenuItem(format);
                        particularFormatMenu.Click += (object sender, EventArgs args) =>
                        {
                            MessageBox.Show(bufer.GetData(format).ToString(), format);
                        };
                        formatsMenu.MenuItems.Add(particularFormatMenu);
                    }

					var buttonWrapper = new BuferHandlersWrapper(this._clipboardBuferService, this, bufer, button, this._form, buferTitle, buferString);

					contextMenu.MenuItems.Add(formatsMenu);
                    contextMenu.MenuItems.Add(new MenuItem("Delete", buttonWrapper.DeleteBufer, Shortcut.Del));
					if (isChangeTextAvailable)
					{
						contextMenu.MenuItems.Add(new MenuItem("Change text", buttonWrapper.ChangeText));
					}
					contextMenu.MenuItems.Add(new MenuItem("Paste", (object sender, EventArgs ars) => {
						SendKeys.Send("~");
					}));
					contextMenu.MenuItems.Add(new MenuItem("Mark as persistent", buttonWrapper.MarkAsPersistent));//This item must be the last
                    button.ContextMenu = contextMenu;
                    button.Width = width;
                }

                button.TabIndex = --buttonIndex;
                y -= BUTTON_HEIGHT;
                button.Location = new Point(0, y);
            }            
        }

        private void RemoveOldButtons(IEnumerable<IDataObject> bufers)
        {
			Logger.Logger.Current.Write("Rendering Handler: Remove Old Buttons");
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