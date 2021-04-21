namespace BuferMAN.Infrastructure
{
    public interface IBuferHandlersBinder
    {
        void Bind(IBufer bufer, IBufermanHost bufermanHost);
    }
}
