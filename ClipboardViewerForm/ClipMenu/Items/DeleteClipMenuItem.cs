using ClipboardBufer;
using ClipboardViewerForm.Properties;
using System;
using System.Windows.Forms;
using Windows;

namespace ClipboardViewerForm.ClipMenu.Items
{
    class DeleteClipMenuItem : MenuItem
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private IDataObject _dataObject;
        private Button _button;

        public DeleteClipMenuItem(IClipboardBuferService clipboardBuferService, IDataObject dataObject, Button button)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._dataObject = dataObject;
            this._button = button;
            this.Text = Resource.MenuDelete;
            this.Shortcut = Shortcut.Del;
            this.Click += this._DeleteBufer;
        }

        private void _DeleteBufer(object sender, EventArgs e)
        {
            var tabIndex = this._button.TabIndex;
            this._clipboardBuferService.RemoveClip(this._dataObject);
            WindowLevelContext.Current.RerenderBufers();

            tabIndex = this._GetNearestTabIndex(tabIndex);

            if (tabIndex > 0)
            {
                var keyboard = new KeyboardEmulator();

                if (tabIndex < this._clipboardBuferService.ClipsCount - tabIndex)
                {
                    new KeyboardEmulator().PressTab((uint)tabIndex);
                }
                else
                {
                    new KeyboardEmulator().HoldDownShift().PressTab((uint)(this._clipboardBuferService.ClipsCount - tabIndex));
                }
            }
        }

        private int _GetNearestTabIndex(int tabIndex)
        {
            if (tabIndex == this._clipboardBuferService.ClipsCount)
            {
                tabIndex -= 1;
            }

            return tabIndex;
        }
    }
}