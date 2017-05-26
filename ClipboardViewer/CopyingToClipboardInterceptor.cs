using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ClipboardViewer
{
	class CopyingToClipboardInterceptor
    {
        private readonly IClipboardBuferService _clipboardBuferService;
		private readonly IEqualityComparer<IDataObject> _comparer;
		private readonly Form _form;
        private readonly IRenderingHandler _renderingHandler;

        public CopyingToClipboardInterceptor(IClipboardBuferService clipboardBuferService, Form form, IRenderingHandler renderingHandler, IEqualityComparer<IDataObject> comparer)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._form = form;
            this._renderingHandler = renderingHandler;
			this._comparer = comparer;
        }

        public void DoOnCtrlC()
        {
			Logger.Logger.Current.Write("On Ctrl+C");

            var currentObject = Clipboard.GetDataObject();

            if (currentObject.GetFormats().Any() && !this._clipboardBuferService.IsLastClip(currentObject))
            {
				Logger.Logger.Current.Write("Is not last clip");
				if (this._clipboardBuferService.Contains(currentObject))
                {
					Logger.Logger.Current.Write("Already contains this clip");
					if (this._clipboardBuferService.IsNotPersistent(currentObject))
					{
						_clipboardBuferService.RemoveClip(currentObject);
					} else
					{
						return;
					}
                }
				
                if (_clipboardBuferService.GetClips().Count() == 30)
                {
					Logger.Logger.Current.Write("More than maximum count. Need to remove the first clip");
					_clipboardBuferService.RemoveClip(_clipboardBuferService.FirstClip);
                }

				Logger.Logger.Current.Write("Add Clip");
				_clipboardBuferService.AddTemporaryClip(currentObject);

                if (this._form.WindowState != FormWindowState.Minimized && this._form.Visible)
                {
                    this._renderingHandler.Render();
                }
            }
        }
    }
}
