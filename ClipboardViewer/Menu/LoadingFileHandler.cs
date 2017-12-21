using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClipboardBufer;

namespace ClipboardViewer.Menu
{
    class LoadingFileHandler : ILoadingFileHandler
    {
        private readonly OpenFileDialog _dialog = new OpenFileDialog();
        private readonly IClipboardWrapper _clipboardWrapper;

        public LoadingFileHandler(IClipboardWrapper clipboardWrapper)
        {
            this._clipboardWrapper = clipboardWrapper;

            this._dialog.Filter = "Текстовые файлы (*.txt)|*.txt";
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
                var fileName = this._dialog.FileName;
                try
                {
                    using (var fileReader = new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read), Encoding.Default))
                    {
                        while (!fileReader.EndOfStream)
                        {
                            var bufer = fileReader.ReadLine();
                            if (!string.IsNullOrWhiteSpace(bufer))
                            {
                                var dataObject = new DataObject("System.String", bufer);
                                Clipboard.SetDataObject(dataObject);
                            }
                        }
                    }
                }
                catch (IOException exc)
                {
                    MessageBox.Show($"There is an error while reading a file {this._dialog.FileName}:\n\n {exc.Message}", "Loading file error");
                }
            }
        }
    }
}
