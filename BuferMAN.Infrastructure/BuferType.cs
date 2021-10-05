using System;

namespace BuferMAN.Infrastructure
{
    [Flags]
    public enum BuferType
    {
        All = 1,
        Temporary = 2,
        Pinned = 4
    }
}
