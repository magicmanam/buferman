using ClipboardViewerForm.Window;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ClipboardBufer;
using Logging;

namespace ClipboardViewerForm
{
	class CopyingToClipboardInterceptor : ICopyingToClipboardInterceptor
    {
        private readonly IClipboardBuferService _clipboardBuferService;
		private readonly IEqualityComparer<IDataObject> _comparer;
		private readonly Form _form;
        private readonly IRenderingHandler _renderingHandler;
        private readonly IClipboardWrapper _clipboardWrapper;

        public CopyingToClipboardInterceptor(IClipboardBuferService clipboardBuferService, Form form, IRenderingHandler renderingHandler, IEqualityComparer<IDataObject> comparer, IClipboardWrapper clipboardWrapper)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._form = form;
            this._renderingHandler = renderingHandler;
			this._comparer = comparer;
            this._clipboardWrapper = clipboardWrapper;
        }

        public void DoOnCtrlC()
        {
            //Here I need undoable context
            var currentObject = this._clipboardWrapper.GetDataObject();

            if (currentObject.GetFormats().Any() && !this._clipboardBuferService.IsLastTemporaryClip(currentObject))
            {
				if (this._clipboardBuferService.Contains(currentObject))
                {
					if (this._clipboardBuferService.IsNotPersistent(currentObject))
					{
						_clipboardBuferService.RemoveClip(currentObject);
					} else
					{
						return;
					}
                }
                
				_clipboardBuferService.AddTemporaryClip(currentObject);

                if (this._form.WindowState != FormWindowState.Minimized && this._form.Visible)
                {
                    this._renderingHandler.Render();
                }
            }
        }
    }
}
