using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ClipboardBufer;
using ClipboardViewerForm.Properties;

namespace ClipboardViewerForm
{
    class LoadingFileHandler : ILoadingFileHandler
    {
        private readonly OpenFileDialog _dialog = new OpenFileDialog();
        private readonly IClipboardWrapper _clipboardWrapper;

        public LoadingFileHandler(IClipboardWrapper clipboardWrapper)
        {
            this._clipboardWrapper = clipboardWrapper;

            this._dialog.Filter = Resource.LoadFileFilter;
            this._dialog.CheckFileExists = true;
            this._dialog.CheckPathExists = true;
            this._dialog.RestoreDirectory = true;
            this._dialog.Multiselect = false;
        }

        public void OnLoadFile(object sender, EventArgs args)
        {
            var result = this._dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.LoadBufersFromFile(this._dialog.FileName);
            }
        }

        public void LoadBufersFromFile(string fileName)
        {
            try
            {
                using (var fileReader = new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read)))
                {
                    while (!fileReader.EndOfStream)
                    {
                        var bufer = fileReader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(bufer))
                        {
                            var dataObject = new DataObject(DataFormats.StringFormat, bufer);
                            this._clipboardWrapper.SetDataObject(dataObject);
                        }
                    }
                }
            }
            catch (IOException exc)
            {
                MessageBox.Show(Resource.LoadFileErrorPrefix + $" {this._dialog.FileName}:\n\n {exc.Message}", Resource.LoadFileErrorTitle);
            }
        }
    }
}
