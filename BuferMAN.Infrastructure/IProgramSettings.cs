namespace BuferMAN.Infrastructure
{
    public interface IProgramSettings
    {
        string DefaultBufersFileName { get; }
        int MaxBufersCount { get; }
        int ExtraBufersCount { get; } // Can not be big, because rendering is too slow cause of auto keyboard emulation.
    }
}
