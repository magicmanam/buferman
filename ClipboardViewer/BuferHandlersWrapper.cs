using ClipboardViewer.Window;
using System;
using System.Drawing;
using System.Windows.Forms;
using ClipboardBufer;
using Logging;
using System.Linq;

namespace ClipboardViewer
{
	class BuferHandlersWrapper : IBuferHandlersWrapper
    {
		private readonly IClipboardBuferService _clipboardBuferService;
		private readonly IRenderingHandler _renderingHandler;
		private readonly IDataObject _dataObject;
		private readonly Button _button;
		private readonly string _originButtonText;
		private readonly ToolTip _mouseOverTooltip;
        private readonly ToolTip _focusTooltip = new ToolTip() { OwnerDraw = true };
		private string _tooltipText;

		public BuferHandlersWrapper(IClipboardBuferService clipboardBuferService, IRenderingHandler renderingHandler, IDataObject dataObject, Button button, Form form, string tooltipTitle, string tooltipText)
		{
			this._clipboardBuferService = clipboardBuferService;
			this._renderingHandler = renderingHandler;
			this._dataObject = dataObject;
			this._button = button;
			this._originButtonText = button.Text;
			this._tooltipText = tooltipText;
            this._focusTooltip.Popup += Tooltip_Popup;
            this._focusTooltip.Draw += Tooltip_Draw;

			var tooltip = new ToolTip() { InitialDelay = 0 };
			tooltip.IsBalloon = true;
			tooltip.SetToolTip(button, tooltipText);
			if (!string.IsNullOrWhiteSpace(tooltipTitle))
			{
				tooltip.ToolTipTitle = tooltipTitle;
				this._focusTooltip.ToolTipTitle = tooltipTitle;
			}

            if (_dataObject.GetFormats().Contains(ClipboardFormats.CUSTOM_IMAGE_FORMAT))
            {
                button.Tag = _dataObject.GetData(ClipboardFormats.CUSTOM_IMAGE_FORMAT) as Image;
                tooltip.IsBalloon = false;
                tooltip.OwnerDraw = true;
                tooltip.Popup += Tooltip_Popup;
                tooltip.Draw += Tooltip_Draw;
            }
            this._mouseOverTooltip = tooltip;

			button.GotFocus += Button_GotFocus;
			button.LostFocus += Button_LostFocus;

			button.Click += new BuferSelectionHandler(form, dataObject, new WindowHidingHandler(form)).DoOnClipSelection;
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
                b.ScaleTransform(0.5f, 0.5f);

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
                e.ToolTipSize = new Size(previewImage.Width / 2, previewImage.Height / 2);
            }
        }

        public void DeleteBufer(object sender, EventArgs e)
		{
			Logger.Write("On Delete Bufer");
			this._clipboardBuferService.RemoveClip(this._dataObject);
			this._renderingHandler.Render();
		}

		public void MarkAsPersistent(object sender, EventArgs e)
		{
			Logger.Write("On Mark As Persistent");
			this._clipboardBuferService.MarkClipAsPersistent(this._dataObject);
			var menuItems = this._button.ContextMenu.MenuItems;
			menuItems[menuItems.Count - 1].Enabled = false;
			this._renderingHandler.Render();
		}

		public void ChangeText(object sender, EventArgs e)
		{
			Logger.Write("On Change Text");

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
				}
				else
				{
					this._button.Font = new Font(this._button.Font, FontStyle.Bold);
				}

				this._mouseOverTooltip.SetToolTip(this._button, newText);
			}
		}

		private void Button_GotFocus(object sender, EventArgs e)
		{
			Logger.Write("Got Focus");
			this._focusTooltip.Show(this._tooltipText, this._button, 2500);
            this._focusTooltip.OwnerDraw = true;


            Logger.Write("After Got Focus");
		}

		private void Button_LostFocus(object sender, EventArgs e)
		{
			Logger.Write("Lost Focus");
			this._focusTooltip.Hide(this._button);
			Logger.Write("After Lost Focus");
		}
	}
}
