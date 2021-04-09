using BuferMAN.Infrastructure.Menu;

namespace BuferMAN.Plugins.PCCleaner
{
    public class PCCleanerPlugin : BufermanPluginBase
    {
        public PCCleanerPlugin() : base(Resource.PCCleanerPlugin)
        {
            this.Enabled = true;
        }

        public override bool Enabled { get; set; }

        public override BufermanMenuItem CreateMainMenuItem()
        {
            return this.BufermanHost.CreateMenuItem(this.Name);
        }
    }
}
