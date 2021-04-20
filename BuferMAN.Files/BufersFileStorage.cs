using BuferMAN.Clipboard;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BuferMAN.Files
{
    internal class BufersFileStorage : IPersistentBufersStorage
    {
        private readonly string _filePath;
        private readonly IBufersFileFormatter _fileFormatter;

        public BufersFileStorage(IBufersFileFormatter fileFormatter, string filePath)
        {
            this._fileFormatter = fileFormatter;
            this._filePath = filePath;
        }

        public event EventHandler<BufersLoadedEventArgs> BufersLoaded;

        public void LoadBufers()
        {
            if (File.Exists(this._filePath))
            {
                try
                {
                    using (var fileReader = BufersFileStorage.GetMultiLanguageFileReader(this._filePath))
                    {
                        var bufers = this._fileFormatter.Parse(fileReader.ReadToEnd());
                        if (bufers.Any())
                        {
                            this.BufersLoaded?.Invoke(this, new BufersLoadedEventArgs(bufers));
                        }
                    }
                }
                catch (IOException exc)
                {
                    throw new ClipboardMessageException(Resource.LoadFileErrorPrefix + $" {this._filePath}:\n\n {exc.Message}", exc)
                    {
                        Title = Resource.LoadFileErrorTitle
                    };
                }
                catch (JsonReaderException exc)
                {
                    throw new ClipboardMessageException(exc.Message, exc);
                }
            }
        }

        public void SaveBufer(BuferItem buferItem)
        {
            if (File.Exists(this._filePath))
            {
                try
                {
                    List<BuferItem> bufers;

                    using (var fileReader = BufersFileStorage.GetMultiLanguageFileReader(this._filePath))
                    {
                        bufers = this._fileFormatter.Parse(fileReader.ReadToEnd()).ToList();
                    }

                    bufers.Add(buferItem);

                    using (var fileWriter = BufersFileStorage.GetMultiLanguageFileWriter(this._filePath))
                    {
                        fileWriter.Write(this._fileFormatter.ToString(bufers));
                    }
                }
                catch (IOException exc)
                {
                    throw new ClipboardMessageException(Resource.LoadFileErrorPrefix + $" {this._filePath}:\n\n {exc.Message}", exc)
                    {
                        Title = Resource.LoadFileErrorTitle
                    };
                }
                catch (JsonReaderException exc)
                {
                    throw new ClipboardMessageException(exc.Message, exc);
                }
            }
        }

        private static StreamReader GetMultiLanguageFileReader(string fileName)
        {
            return new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read), Encoding.Default);
        }

        private static StreamWriter GetMultiLanguageFileWriter(string fileName)
        {
            return new StreamWriter(fileName, false, Encoding.Default);
        }
    }
}
