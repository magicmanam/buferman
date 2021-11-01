using System.Collections.Generic;

namespace BuferMAN.Infrastructure.Menu
{
    public interface IMainMenuGenerator
    {
        IEnumerable<BufermanMenuItem> GenerateMainMenu(IBufermanApplication bufermanApplication, IBufermanHost bufermanHost);// TODO (s) remove BufermanApplication from parameters
    }
}
