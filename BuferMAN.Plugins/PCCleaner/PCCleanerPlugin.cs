using BuferMAN.Infrastructure.Menu;

namespace BuferMAN.Plugins.PCCleaner
{
    public class PCCleanerPlugin : BufermanPluginBase
    {
        public PCCleanerPlugin()
        {
            this.Enabled = false;
        }

        public override bool Enabled { get; set; }

        public override BufermanMenuItem CreateMainMenuItem()
        {
            return this.BufermanHost.CreateMenuItem(this.Name);
        }

        public override string Name
        {
            get
            {
                return Resource.PCCleanerPlugin;
            }
        }
    }
}
