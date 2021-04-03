using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Plugins;

namespace BuferMAN.Plugins.Scripts
{
    public class ScriptsPlugin : BufermanPluginBase
    {
        public ScriptsPlugin() : base(Resource.ScriptsPlugin) { }

        public override void InitializeMainMenu(BuferMANMenuItem menuItem)
        {
            menuItem.AddMenuItem(this.BufermanHost.CreateMenuItem(this.Name));
        }
    }
}
