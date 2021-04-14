using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Plugins;

namespace BuferMAN.Plugins
{
    public abstract class BufermanPluginBase : IBufermanPlugin
    {
        protected IBufermanHost BufermanHost { get; set; }

        public BufermanPluginBase(){ }

        public virtual void Initialize(IBufermanHost bufermanHost)
        {
            this.BufermanHost = bufermanHost;
        }

        public abstract BufermanMenuItem CreateMainMenuItem();

        public virtual BufermanMenuItem CreateBuferContextMenuItem()
        {
            return null;
        }

        public virtual void UpdateBuferContextMenu(BuferContextMenuModel contextMenuModel)
        {
            return;
        }

        public abstract string Name { get; }
        public abstract bool Enabled { get; set; }
    }
}
