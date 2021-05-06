using BuferMAN.Infrastructure.Menu;

namespace BuferMAN.Plugins.Scripts
{
    public class ScriptsPlugin : BufermanPluginBase
    {
        public ScriptsPlugin()
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
                return Resource.ScriptsPlugin;
            }
        }
    }
}
