using BuferMAN.Infrastructure.Storage;
using System;

namespace BuferMAN.Infrastructure.Files
{
    public interface IUserFileSelector
    {
        void TrySelectBufersStorage(Action<IPersistentBufersStorage> action);
    }
}
