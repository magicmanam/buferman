using BuferMAN.Models;
using BuferMAN.View;

namespace BuferMAN.Infrastructure
{
    public interface IMapper
    {
        BuferItem Map(BuferViewModel buferViewModel);
    }
}
