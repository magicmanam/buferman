using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Files;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Models;
using System.Linq;

namespace BuferMAN.Storage
{
    public class SessionManager : ISessionManager
    {
        private const string SESSION_FILE_PREFIX = "session_state";

        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IBufersStorageFactory _bufersStorageFactory;
        private readonly IProgramSettingsGetter _settings;
        private readonly IFileStorage _fileStorage;
        private readonly ITime _time;

        public SessionManager(
            IClipboardBuferService clipboardBuferService,
            IProgramSettingsGetter settings,
            IBufersStorageFactory bufersStorageFactory,
            IFileStorage fileStorage,
            ITime time)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._settings = settings;
            this._bufersStorageFactory = bufersStorageFactory;
            this._fileStorage = fileStorage;
            this._time = time;
        }

        public void RestoreSession()
        {
            var storage = this._bufersStorageFactory.Create(BufersStorageType.JsonFile, this._GetLatestSessionSavedFilePath());
            storage.LoadBufers();
        }

        public void SaveSession()
        {
            var buferItems = this._clipboardBuferService.GetTemporaryBufers()
                .Where(b => b.Clip.IsStringObject())
                .Select(b => b.ToModel())
                .Union(this._clipboardBuferService
                                   .GetPinnedBufers()
                                   .Where(b => b.Clip.IsStringObject())
                                   .Select(b => b.ToModel()))
                .ToList();

            if (buferItems.Any())
            {
                var now = this._time.LocalTime;
                var sessionFile = this._fileStorage.CombinePaths(
                    this._fileStorage.DataDirectory,
                    this._settings.SessionsSubDirectory,
                    $"{SessionManager.SESSION_FILE_PREFIX}_{now.Year}_{now.Month}_{now.Day}_{now.Hour}_{now.Minute}_{now.Second}_{now.Millisecond}_{buferItems.Count()}.json");

                var storage = this._bufersStorageFactory.CreateStorageByFileExtension(sessionFile);

                storage.SaveBufers(buferItems);
            }
        }

        public bool IsLatestSessionSaved()
        {
            return this._GetLatestSessionSavedFilePath() != null;
        }

        private string _GetLatestSessionSavedFilePath()
        {
            var sessionsDirectory = this._fileStorage.CombinePaths(
                this._fileStorage.DataDirectory,
                this._settings.SessionsSubDirectory);

            if (this._fileStorage.DirectoryExists(sessionsDirectory))
            {
                return this._fileStorage.GetFiles(sessionsDirectory, $"{SessionManager.SESSION_FILE_PREFIX}_*.json").Max();
            }
            else
            {
                return null;
            }
        }
    }
}
