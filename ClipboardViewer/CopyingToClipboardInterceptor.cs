using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardViewer
{
    class CopyingToClipboardInterceptor
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly Form _form;
        private readonly IRenderingHandler _renderingHandler;

        public CopyingToClipboardInterceptor(IClipboardBuferService clipboardBuferService, Form form, IRenderingHandler renderingHandler)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._form = form;
            this._renderingHandler = renderingHandler;
        }

        public void DoOnCtrlC()
        {
            var currentObject = Clipboard.GetDataObject();

            if (!this._clipboardBuferService.IsLastClip(currentObject))
            {
                if (_clipboardBuferService.Contains(currentObject))
                {
                    _clipboardBuferService.RemoveClip(currentObject);
                }

                if (_clipboardBuferService.GetClips().Count() == 30)
                {
                    _clipboardBuferService.RemoveClip(_clipboardBuferService.FirstClip);
                }

                _clipboardBuferService.AddClip(currentObject);

                if (this._form.WindowState != FormWindowState.Minimized && this._form.Visible)
                {
                    this._renderingHandler.Render();
                }
            }
        }
    }
}
