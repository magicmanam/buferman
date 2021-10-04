using BuferMAN.View;
using System.Collections.Generic;

namespace BuferMAN.Infrastructure.Window
{
    public interface IRenderingHandler
    {
        void Render(IBufermanHost bufermanHost, IEnumerable<BuferViewModel> temporaryBuferViewModels, IEnumerable<BuferViewModel> pinnedBuferViewModels);
    }
}