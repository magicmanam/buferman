using System;
using System.IO;
using System.Text;
using BuferMAN.Infrastructure;
using System.Windows.Forms;
using BuferMAN.Clipboard;
using BuferMAN.Files.Properties;
using System.Collections;
using BuferMAN.Storage;
using System.Collections.Generic;

namespace BuferMAN.Files
{
    public class LoadingFileHandler : ILoadingFileHandler
    {
        private readonly OpenFileDialog _dialog = new OpenFileDialog();
        private readonly IIDataObjectHandler _dataObjectHandler;
        private readonly IBufersFileParser _fileParser;
        private readonly IProgramSettings _settings;

        public LoadingFileHandler(IIDataObjectHandler dataObjectHandler, IBufersFileParser fileParser, IProgramSettings settings)
        {
            this._dataObjectHandler = dataObjectHandler;
            this._fileParser = fileParser;
            this._settings = settings;

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
                foreach(var buferEntry in this.LoadBufersFromFile(this._dialog.FileName))
                {
                        var dataObject = new DataObject();
                        dataObject.SetText(buferEntry.Data as string);
                        dataObject.SetData(ClipboardFormats.FROM_FILE_FORMAT, null);
                        this._dataObjectHandler.HandleDataObject(dataObject);
                }
            }
        }

        public IEnumerable<BuferItem> LoadBufersFromFile(string fileName)
        {
            try
            {
                using (var fileReader = LoadingFileHandler.GetMultiLanguageFileReader(fileName))
                {
                    return this._fileParser.Parse(fileReader);
                }
            }
            catch (IOException exc)
            {
                MessageBox.Show(Resource.LoadFileErrorPrefix + $" {this._dialog.FileName}:\n\n {exc.Message}", Resource.LoadFileErrorTitle);
            }

            return new List<BuferItem>();
        }

        private static StreamReader GetMultiLanguageFileReader(string fileName)
        {
            return new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read), Encoding.Default);
        }
    }
}
