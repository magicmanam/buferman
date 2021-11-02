using System.Collections.Generic;

namespace BuferMAN.Infrastructure.Menu
{
    public interface IMainMenuGenerator
    {
        IEnumerable<BufermanMenuItem> GenerateMainMenu(IBufermanHost bufermanHost);
    }
}
