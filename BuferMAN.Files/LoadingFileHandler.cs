using System;
using System.IO;
using System.Text;
using BuferMAN.Infrastructure;
using System.Windows.Forms;
using BuferMAN.Clipboard;
using BuferMAN.Files.Properties;
using BuferMAN.Infrastructure.Storage;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace BuferMAN.Files
{
    public class LoadingFileHandler : ILoadingFileHandler
    {
        public event EventHandler<BufersLoadedEventArgs> BufersLoaded;

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
                this.LoadBufersFromFile(this._dialog.FileName);
            }
        }

        public void LoadBufersFromFile(string fileName)
        {
            try
            {
                using (var fileReader = LoadingFileHandler.GetMultiLanguageFileReader(fileName))
                {
                    var bufers =  this._fileParser.Parse(fileReader);
                    if (bufers.Any())
                    {
                        this.BufersLoaded?.Invoke(this, new BufersLoadedEventArgs(bufers));
                    }
                }
            }
            catch (IOException exc)
            {
                throw new ClipboardMessageException(Resource.LoadFileErrorPrefix + $" {this._dialog.FileName}:\n\n {exc.Message}", exc)
                {
                    Title = Resource.LoadFileErrorTitle
                };
            }
            catch (JsonReaderException exc)
            {
                throw new ClipboardMessageException(exc.Message, exc);
            }
        }

        private static StreamReader GetMultiLanguageFileReader(string fileName)
        {
            return new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read), Encoding.Default);
        }
    }
}
