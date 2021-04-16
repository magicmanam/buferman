using System;
using System.Windows.Forms;
using BuferMAN.Files.Properties;
using BuferMAN.Infrastructure.Storage;

namespace BuferMAN.Files
{
    public class UserFileStorageSelector : IUserFileSelector
    {
        private readonly OpenFileDialog _dialog = new OpenFileDialog();
        private readonly IBufersStorageFactory _bufersStorageFactory;

        public UserFileStorageSelector(IBufersStorageFactory bufersStorageFactory)
        {
            this._bufersStorageFactory = bufersStorageFactory;

            this._dialog.Filter = Resource.LoadFileFilter;
            this._dialog.CheckFileExists = true;
            this._dialog.CheckPathExists = true;
            this._dialog.RestoreDirectory = true;
            this._dialog.Multiselect = false;
        }

        public void TrySelectBufersStorage(Action<IPersistentBufersStorage> action)
        {
            var result = this._dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                var storage = this._bufersStorageFactory.CreateStorageByFileExtension(this._dialog.FileName);
                // TODO (s) add this file into recent files and display in main and context menus
                action(storage);
            }
        }
    }
}
