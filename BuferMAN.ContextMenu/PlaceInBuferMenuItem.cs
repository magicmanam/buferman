using BuferMAN.Clipboard;
using BuferMAN.ContextMenu.Properties;
using BuferMAN.Infrastructure;
using System;
using System.Windows.Forms;
using Windows;

namespace BuferMAN.ContextMenu
{
    public class PlaceInBuferMenuItem : MenuItem
    {
        private readonly IClipboardWrapper _clipboardWrapper;
        private IDataObject _dataObject;

        public PlaceInBuferMenuItem(IClipboardWrapper clipboardWrapper, IDataObject dataObject)
        {
            this._clipboardWrapper = clipboardWrapper;
            this._dataObject = dataObject;
            this.Text = Resource.MenuPlaceInBufer;
            this.Shortcut = Shortcut.CtrlC;
            this.Click += this._PlaceInBufer;
        }

        private void _PlaceInBufer(object sender, EventArgs e)
        {
            this._clipboardWrapper.SetDataObject(this._dataObject);
        }
    }
}