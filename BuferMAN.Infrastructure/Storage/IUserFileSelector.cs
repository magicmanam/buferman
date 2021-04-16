using System;

namespace BuferMAN.Infrastructure.Storage
{
    public interface IUserFileSelector
    {
        void TrySelectBufersStorage(Action<IPersistentBufersStorage> action);
    }
}
