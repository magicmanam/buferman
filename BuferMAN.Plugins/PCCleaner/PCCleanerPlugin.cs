using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Plugins;

namespace BuferMAN.Plugins.PCCleaner
{
    public class PCCleanerPlugin : BufermanPluginBase
    {
        public PCCleanerPlugin() : base(Resource.PCCleanerPlugin) { }

        public override void InitializeMainMenu(BuferMANMenuItem menuItem)
        {
            menuItem.AddMenuItem(this.BufermanHost.CreateMenuItem(this.Name));
        }
    }
}
