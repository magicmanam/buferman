using ClipboardViewerForm.Window;
using System;
using System.Drawing;
using System.Windows.Forms;
using ClipboardBufer;
using Logging;
using System.Linq;
using System.IO;
using ClipboardViewerForm.ClipMenu;

namespace ClipboardViewerForm
{
	class BuferHandlersWrapper
    {
        private const int TOOLTIP_DURATION = 2500;
        private readonly IClipboardBuferService _clipboardBuferService;
		private readonly IRenderingHandler _renderingHandler;
		private readonly IDataObject _dataObject;
        private readonly IBuferSelectionHandler _buferSelectionHandler;
		private readonly Button _button;
        private readonly ToolTip _focusTooltip = new ToolTip() { OwnerDraw = false };
		private string _tooltipText;
        private const float IMAGE_SCALE = 0.75f;

        public BuferHandlersWrapper(IClipboardBuferService clipboardBuferService, IRenderingHandler renderingHandler, IDataObject dataObject, Button button, Form form, IClipMenuGenerator clipMenuGenerator, IBuferSelectionHandler buferSelectionHandler)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._renderingHandler = renderingHandler;
            this._dataObject = dataObject;
            this._button = button;
            this._buferSelectionHandler = buferSelectionHandler;

            var buferTextRepresentation = dataObject.GetData(ClipboardFormats.UNICODE_STRING_FORMAT) as string;
            var isChangeTextAvailable = true;
            string buferTitle = null;
            if (buferTextRepresentation == null)
            {
                var files = dataObject.GetData(ClipboardFormats.FILE_FORMAT) as string[];
                if (files != null && files.Length > 0)
                {
                    isChangeTextAvailable = false;

                    if (files.Length == 1)
                    {
                        buferTitle = $"<< File >>";
                    }
                    else
                    {
                        buferTitle = $"<< Files ({files.Length}) >>";
                    }

                    var folder = Path.GetDirectoryName(files.First());
                    buferTextRepresentation += folder + " " + Environment.NewLine + Environment.NewLine;
                    buferTextRepresentation += string.Join(Environment.NewLine, files.Select(f => Path.GetFileName(f)).ToList());

                    button.BackColor = Color.Brown;
                } else {
                    var isBitmap = dataObject.GetFormats().Contains(ClipboardFormats.CUSTOM_IMAGE_FORMAT);
                    if (isBitmap)
                    {
                        isChangeTextAvailable = false;
                        buferTextRepresentation = "<< Image >>";
                        button.Font = new Font(button.Font, FontStyle.Italic | FontStyle.Bold);
                    }
                    else
                    {
                        buferTextRepresentation = dataObject.GetData(ClipboardFormats.TEXT_STRING_FORMAT) as string;
                    }
                }
            }

            string buttonText = buferTitle ?? buferTextRepresentation;
            if (string.IsNullOrWhiteSpace(buttonText))
            {
                buttonText = buttonText == null ? "<< Not a text >>" : $"<< {buttonText.Length}   white spaces >>";
                button.Font = new Font(button.Font, FontStyle.Italic | FontStyle.Bold);
                isChangeTextAvailable = false;
            }

            this._tooltipText = buferTextRepresentation;
            button.Text = buttonText.Trim();

            string originBuferText = button.Text;

            var tooltip = new ToolTip() { InitialDelay = 0 };
            tooltip.IsBalloon = true;
            tooltip.SetToolTip(button, buferTextRepresentation);
            if (!string.IsNullOrWhiteSpace(buferTitle))
            {
                tooltip.ToolTipTitle = buferTitle;
                this._focusTooltip.ToolTipTitle = buferTitle;
            }

            if (this._dataObject.GetFormats().Contains(ClipboardFormats.CUSTOM_IMAGE_FORMAT))
            {
                button.Tag = this._dataObject.GetData(ClipboardFormats.CUSTOM_IMAGE_FORMAT) as Image;
                tooltip.IsBalloon = false;
                tooltip.OwnerDraw = true;
                tooltip.Popup += Tooltip_Popup;
                tooltip.Draw += Tooltip_Draw;
            }

            button.GotFocus += Bufer_GotFocus;
            button.LostFocus += Bufer_LostFocus;

            button.Click += this._buferSelectionHandler.DoOnClipSelection;

            button.ContextMenu = clipMenuGenerator.GenerateContextMenu(this._dataObject, button, originBuferText, this._tooltipText, tooltip, isChangeTextAvailable);
        }

        private void Tooltip_Draw(object sender, DrawToolTipEventArgs e)
        {
            // to set the tag for each button or object
            Control parent = e.AssociatedControl;
            Image preview = parent.Tag as Image;

            if (preview != null)
            {
                Graphics g = e.Graphics;

                //create your own custom brush to fill the background with the image
                TextureBrush b = new TextureBrush(new Bitmap(preview));
                b.ScaleTransform(IMAGE_SCALE, IMAGE_SCALE);

                g.FillRectangle(b, e.Bounds);
                b.Dispose();
            }
        }

        private void Tooltip_Popup(object sender, PopupEventArgs e)
        {
            Control parent = e.AssociatedControl;
            var previewImage = parent.Tag as Image;

            if (previewImage != null)
            {
                e.ToolTipSize = new Size((int) (previewImage.Width * IMAGE_SCALE), (int) (previewImage.Height * IMAGE_SCALE));
            }
        }

        private void Bufer_GotFocus(object sender, EventArgs e)
		{
			this._focusTooltip.Show(this._tooltipText, this._button, TOOLTIP_DURATION);
		}

		private void Bufer_LostFocus(object sender, EventArgs e)
		{
			this._focusTooltip.Hide(this._button);
		}
	}
}
