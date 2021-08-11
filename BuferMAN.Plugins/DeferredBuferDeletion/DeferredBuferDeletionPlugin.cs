using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Menu;
using System;

namespace BuferMAN.Plugins.DeferredBuferDeletion
{
    public class DeferredBuferDeletionPlugin : BufermanPluginBase
    {
        public override string Name
        {
            get
            {
                return Resource.DeferredBuferPlugin;
            }
        }

        public DeferredBuferDeletionPlugin()
        {
            this.Available = true;
            this.Enabled = true;
        }

        public override BufermanMenuItem CreateMainMenuItem()
        {
            return this.BufermanHost.CreateMenuItem(this.Name, this._DeferredDeletionMenuItem_Click);
        }

        public override void UpdateBuferContextMenu(BuferContextMenuState contextMenuState)
        {
            new DeferredBuferDeletionWrapper(contextMenuState, this.BufermanHost);
        }

        private void _DeferredDeletionMenuItem_Click(object sender, EventArgs e)
        {
            this.Enabled = !this.Enabled;
        }
    }
}
