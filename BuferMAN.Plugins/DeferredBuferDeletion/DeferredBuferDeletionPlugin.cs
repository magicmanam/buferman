using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Menu;
using System;

namespace BuferMAN.Plugins.DeferredBuferDeletion
{
    public class DeferredBuferDeletionPlugin : BufermanPluginBase
    {
        private readonly ITime _time;
        private BufermanMenuItem _mainMenuItem;

        public override string Name
        {
            get
            {
                return Resource.DeferredBuferPlugin;
            }
        }

        public DeferredBuferDeletionPlugin(ITime time)
        {
            this._time = time;

            this.Available = true;
            this.Enabled = true;
        }

        public override BufermanMenuItem CreateMainMenuItem()
        {
            this._mainMenuItem = this.BufermanHost.CreateMenuItem(() => this.Name, this._DeferredDeletionMenuItem_Click);
            this._mainMenuItem.Checked = this.Available && this.Enabled;

            return this._mainMenuItem;
        }

        public override void UpdateBuferItem(BuferContextMenuState contextMenuState)
        {
            new DeferredBuferDeletionWrapper(contextMenuState, this.BufermanHost, this._time);
        }

        private void _DeferredDeletionMenuItem_Click(object sender, EventArgs e)
        {
            this.Enabled = !this.Enabled;
            this._mainMenuItem.Checked = this.Enabled;
        }
    }
}
