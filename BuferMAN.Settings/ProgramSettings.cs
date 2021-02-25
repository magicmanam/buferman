using BuferMAN.Infrastructure;

namespace BuferMAN.Settings
{
    public class ProgramSettings : IProgramSettings
    {
        public string DefaultBufersFileName => "bufers.txt";

        public int MaxBufersCount => 30;

        public int ExtraBufersCount => 25;
    }
}
