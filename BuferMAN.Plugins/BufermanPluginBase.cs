using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Plugins;
using System;

namespace BuferMAN.Plugins
{
    public abstract class BufermanPluginBase : IBufermanPlugin
    {
        private bool _enabled;

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

        public virtual void UpdateBuferContextMenu(BuferContextMenuState contextMenuModel)
        {
            return;
        }

        public abstract string Name { get; }

        public bool Enabled
        {
            get
            {
                return this.Available ? this._enabled : throw new InvalidOperationException();
            }
            set
            {
                if (this.Available)
                {
                    this._enabled = value;
                    this.OnEnableChanged();
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public bool Available { get; protected set; }

        protected virtual void OnEnableChanged() { }
    }
}
