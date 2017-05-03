﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardViewer
{
    class WindowActivationHandler
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly Form _form;

        private readonly RenderingHandler _renderingHandler;

        public WindowActivationHandler(IClipboardBuferService clipboadService, Form form, RenderingHandler renderingHandler)
        {
            _clipboardBuferService = clipboadService;
            _form = form;
            _renderingHandler = renderingHandler;

        }
                
        public void OnActivated(object sender, EventArgs e)
        {
            this._form.WindowState = FormWindowState.Normal;
            this._form.Visible = true;

            this._renderingHandler.Render();
        }        
    }
}
