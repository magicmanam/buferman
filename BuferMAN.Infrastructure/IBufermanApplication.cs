namespace BuferMAN.Infrastructure
{
    public interface IBufermanApplication
    {
        void RunInHost(IBufermanHost bufermanHost);
    }
}
