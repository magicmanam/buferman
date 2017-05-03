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
        
        private const int BUTTON_HEIGHT = 25;

        public RenderingHandler(Form form, IClipboardBuferService clipboardBuferService)
        {
            this._form = form;
            this._clipboardBuferService = clipboardBuferService;
        }

        public void Render()
        {
            var bufers = _clipboardBuferService.GetClips().ToArray();
            
            int y = bufers.Length * BUTTON_HEIGHT;
            int width = _form.ClientRectangle.Width;
            
            this.RemoveOldButtons(bufers);
            int buttonIndex = bufers.Length;

            foreach (var bufer in bufers)
            {
                Button button;
                if (this._buttonsMap.ContainsKey(bufer))
                {
                    button = this._buttonsMap[bufer];                    
                }
                else
                {
                    var buferString = bufer.GetData("System.String") as string;
                    button = new Button() { TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0) };
                    if (buferString == null)
                    {
                        var files = bufer.GetData("FileDrop") as Array;
                        if (files != null && files.Length > 0)
                        {
                            var firstFileName = files.GetValue(0) as string;
                            if (firstFileName.Length > 30)
                            {
                                firstFileName = Path.GetFileName(firstFileName);
                            }

                            if (files.Length == 1)
                            {
                                buferString = $"<< File >>: {firstFileName}";
                            }
                            else
                            {
                                buferString = $"<< Files ({files.Length}) >>: {firstFileName} ...";
                            }

                            button.BackColor = Color.Brown;
                        }
                    }

                    if (buferString == null)
                    {
                        var isBitmap = bufer.GetFormats().Contains("Bitmap");
                        if (isBitmap)
                        {
                            buferString = "<< Image >>";
                            button.Font = new Font(button.Font, FontStyle.Italic | FontStyle.Bold);
                        }
                    }

                    button.Text = buferString;
                    
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

                    contextMenu.MenuItems.Add(formatsMenu);
                    contextMenu.MenuItems.Add(new MenuItem("Delete", new BuferMenuHandlers(this._clipboardBuferService, this, bufer).DeleteBufer, Shortcut.Del));
                    button.ContextMenu = contextMenu;
                    button.Width = width;
                    button.Click += new ClipSelectionHandler(_form, bufer).DoOnClipSelection;
                    button.GotFocus += Button_GotFocus;
                    var tooltip = new ToolTip() { InitialDelay = 0 };
                    tooltip.IsBalloon = true;
                    tooltip.SetToolTip(button, button.Text);                    
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
                if (!bufers.Contains(key))
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

        class BuferMenuHandlers
        {
            private IClipboardBuferService _clipboardBuferService;
            private RenderingHandler _renderingHandler;
            private readonly IDataObject _dataObject;

            public BuferMenuHandlers(IClipboardBuferService clipboardBuferService, RenderingHandler renderingHandler, IDataObject dataObject)
            {
                this._clipboardBuferService = clipboardBuferService;
                this._renderingHandler = renderingHandler;
                this._dataObject = dataObject;
            }

            public void DeleteBufer(object sender, EventArgs e)
            {                
                this._clipboardBuferService.RemoveClip(this._dataObject);
                this._renderingHandler.Render();
            }
        }
        
        private ToolTip _tooltip;
        private Button _lastFocusedButton;
        private void Button_GotFocus(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            
            this._tooltip?.Hide(_lastFocusedButton);
            this._tooltip = new ToolTip { InitialDelay = 0 };
            this._tooltip.Show(button.Text, button, 3000);
            this._lastFocusedButton = button;
        }
    }
}