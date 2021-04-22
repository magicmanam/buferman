namespace BuferMAN.Infrastructure
{
    public interface IBufermanApplication
    {
        void RunInHost(IBufermanHost bufermanHost);
        bool ShouldCatchCopies { get; set; }
        IBufermanHost Host { get; }
    }
}
