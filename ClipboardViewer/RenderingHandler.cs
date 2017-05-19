using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardViewer
{
    class RenderingHandler : IRenderingHandler
    {
        private readonly Form _form;
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IDictionary<IDataObject, Button> _buttonsMap = new Dictionary<IDataObject, Button>(30);
		private readonly IEqualityComparer<IDataObject> _comparer;
        
        private const int BUTTON_HEIGHT = 25;

        public RenderingHandler(Form form, IClipboardBuferService clipboardBuferService, IEqualityComparer<IDataObject> comparer)
        {
            this._form = form;
            this._clipboardBuferService = clipboardBuferService;
			this._comparer = comparer;
        }

        public void Render()
        {
			Logger.Logger.Current.Write("On render");
            var bufers = _clipboardBuferService.GetClips().ToArray();
            
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
                    var buferString = bufer.GetData("Text") as string;
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

							buferString = string.Join(Environment.NewLine, files.ToList());

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

                    button.Text = buferTitle ?? buferString;
                    
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

        class BuferHandlersWrapper
        {
            private readonly IClipboardBuferService _clipboardBuferService;
            private readonly RenderingHandler _renderingHandler;
            private readonly IDataObject _dataObject;
			private readonly Button _button;
			private readonly string _originButtonText;
			private readonly ToolTip _mouseOverTooltip;
			private readonly ToolTip _focusTooltip = new ToolTip();
			private string _tooltipText;
			
			public BuferHandlersWrapper(IClipboardBuferService clipboardBuferService, RenderingHandler renderingHandler, IDataObject dataObject, Button button, Form form, string tooltipTitle, string tooltipText)
            {
                this._clipboardBuferService = clipboardBuferService;
                this._renderingHandler = renderingHandler;
                this._dataObject = dataObject;
                this._button = button;
				this._originButtonText = button.Text;
				this._tooltipText = tooltipText;

				var tooltip = new ToolTip() { InitialDelay = 0 };
				tooltip.IsBalloon = true;
				tooltip.SetToolTip(button, tooltipText);
				if (!string.IsNullOrWhiteSpace(tooltipTitle))
				{
					tooltip.ToolTipTitle = tooltipTitle;
					this._focusTooltip.ToolTipTitle = tooltipTitle;
				}
				this._mouseOverTooltip = tooltip;

				button.GotFocus += Button_GotFocus;
				button.LostFocus += Button_LostFocus;

				button.Click += new ClipSelectionHandler(form, dataObject).DoOnClipSelection;
			}

            public void DeleteBufer(object sender, EventArgs e)
            {
                this._clipboardBuferService.RemoveClip(this._dataObject);
                this._renderingHandler.Render();
            }

			public void ChangeText(object sender, EventArgs e)
			{
				var newText = Microsoft.VisualBasic.Interaction.InputBox($"Enter a new text for this bufer. It can be useful to hide copied passwords or alias some enourmous text. Primary button value was \"{this._originButtonText}\".",
					   "Change bufer's text",
					   this._button.Text);

				if (!string.IsNullOrWhiteSpace(newText) && newText != this._button.Text)
				{
					this._button.Text = newText;
					this._tooltipText = newText;
					if (newText == this._originButtonText)
					{
						MessageBox.Show("Bufer alias was returned to its primary value");
						this._button.Font = new Font(this._button.Font, FontStyle.Regular);
					} else
					{
						this._button.Font = new Font(this._button.Font, FontStyle.Bold);
					}

					this._mouseOverTooltip.SetToolTip(this._button, newText);
				}
			}

			private void Button_GotFocus(object sender, EventArgs e)
			{
				this._focusTooltip.Show(this._tooltipText, this._button, 2500);
			}

			private void Button_LostFocus(object sender, EventArgs e)
			{
				this._focusTooltip.Hide(this._button);
			}
		}
    }
}