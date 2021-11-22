using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Menu;
using System;

namespace BuferMAN.Plugins.BigTextBufer
{
    public class BigTextBuferPlugin : BufermanPluginBase
    {
        private BufermanMenuItem _mainMenuItem;

        private const int MaxBuferPresentationLength = 2300;//Limits: low 2000, high 5000

        public override string Name
        {
            get
            {
                return Resource.BigTextBuferPlugin;
            }
        }

        public BigTextBuferPlugin()
        {
            this.Available = true;
            this.Enabled = true;
        }

        public override BufermanMenuItem CreateMainMenuItem()
        {
            this._mainMenuItem = this.BufermanHost.CreateMenuItem(() => this.Name, this._BigTextBuferMenuItem_Click);
            this._mainMenuItem.Checked = this.Available && this.Enabled;

            return this._mainMenuItem;
        }

        public override void UpdateBuferItem(BuferContextMenuState contextMenuState)
        {
            if (contextMenuState.Bufer.ViewModel.IsChangeTextAvailable)
            {
                var buferTextRepresentation = contextMenuState.Bufer.ViewModel.TextRepresentation;

                if (buferTextRepresentation != null && buferTextRepresentation.Length > BigTextBuferPlugin.MaxBuferPresentationLength)
                {
                    buferTextRepresentation = this._ReduceRepresentation(buferTextRepresentation);

                    contextMenuState.Bufer.ViewModel.Representation = buferTextRepresentation;
                    contextMenuState.Bufer.SetMouseOverToolTip(buferTextRepresentation);
                    contextMenuState.Bufer.ViewModel.TextRepresentation = buferTextRepresentation;
                    contextMenuState.Bufer.ViewModel.TooltipTitle = Resource.BigTextBufer;

                    contextMenuState.Bufer.MouseOverTooltip.ToolTipTitle = contextMenuState.Bufer.ViewModel.TooltipTitle;
                    contextMenuState.Bufer.FocusTooltip.ToolTipTitle = contextMenuState.Bufer.ViewModel.TooltipTitle;
                }
            }
        }

        private void _BigTextBuferMenuItem_Click(object sender, EventArgs e)
        {
            this.Enabled = !this.Enabled;
            this._mainMenuItem.Checked = this.Enabled;
        }

        private string _ReduceRepresentation(string representation)
        {
            return $"{representation.Substring(0, BigTextBuferPlugin.MaxBuferPresentationLength - 300)}{Environment.NewLine}{Environment.NewLine}...";
        }
    }
}
