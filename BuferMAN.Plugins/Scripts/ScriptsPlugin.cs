using BuferMAN.Infrastructure.Menu;

namespace BuferMAN.Plugins.Scripts
{
    public class ScriptsPlugin : BufermanPluginBase
    {
        public ScriptsPlugin() : base(Resource.ScriptsPlugin)
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
