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
			Logger.Write("On Ctrl+C");

            var currentObject = this._clipboardWrapper.GetDataObject();

            if (!this._clipboardBuferService.IsLastTemporaryClip(currentObject))
            {
				Logger.Write("Is not last clip");
				if (this._clipboardBuferService.Contains(currentObject))
                {
					Logger.Write("Already contains this clip");
					if (this._clipboardBuferService.IsNotPersistent(currentObject))
					{
						_clipboardBuferService.RemoveClip(currentObject);
					} else
					{
						return;
					}
                }

				Logger.Write("Add Clip");
				_clipboardBuferService.AddTemporaryClip(currentObject);

                if (this._form.WindowState != FormWindowState.Minimized && this._form.Visible)
                {
                    this._renderingHandler.Render();
                }
            }
        }
    }
}
