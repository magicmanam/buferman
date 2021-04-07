using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Plugins;

namespace BuferMAN.Plugins
{
    public abstract class BufermanPluginBase : IBufermanPlugin
    {
        protected IBufermanHost BufermanHost { get; set; }

        public BufermanPluginBase() { }

        public BufermanPluginBase(string name)
        {
            this.Name = name;
        }

        public virtual void InitializeHost(IBufermanHost bufermanHost)
        {
            this.BufermanHost = bufermanHost;
        }

        public abstract void InitializeMainMenu(BufermanMenuItem menuItem);

        public virtual string Name { get; protected set; }
    }
}
