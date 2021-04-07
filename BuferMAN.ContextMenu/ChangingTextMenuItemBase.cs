using BuferMAN.ContextMenu.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;
using magicmanam.Windows;
using BuferMAN.View;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure;

namespace BuferMAN.ContextMenu
{
    public abstract class ChangingTextMenuItemBase
    {
        protected IBufermanHost BufermanHost { get; set; }

        protected BufermanMenuItem MenuItem { get; private set; }

        protected Button Button { get; private set; }
        protected ToolTip MouseOverTooltip { get; private set; }

        protected ChangingTextMenuItemBase(BufermanMenuItem menuItem, Button button, ToolTip mouseOverTooltip, IBufermanHost bufermanHost)
        {
            this.MenuItem = menuItem;
            this.Button = button;
            this.MouseOverTooltip = mouseOverTooltip;
            this.BufermanHost = bufermanHost;
        }

        public event EventHandler<TextChangedEventArgs> TextChanged;

        public void TryChangeText(string newText)
        {
            if (!string.IsNullOrWhiteSpace(newText) && newText != this.Button.Text)
            {
                this.Button.Text = newText;
                var buttonViewModel = this.Button.Tag as BuferViewModel;
                buttonViewModel.Representation = newText;
                ChangingTextMenuItemBase._UpdateFocusTooltip();

                bool isOriginText = newText == (this.Button.Tag as BuferViewModel).OriginBuferText;

                if (isOriginText)
                {
                    this.Button.Font = new Font(this.Button.Font, FontStyle.Regular);
                    this.BufermanHost.UserInteraction.ShowPopup(Resource.BuferAliasReturned, Resource.ChangeTextTitle);
                }
                else
                {
                    this.Button.Font = new Font(this.Button.Font, FontStyle.Bold);
                }

                this.TextChanged?.Invoke(this, new TextChangedEventArgs(isOriginText));
                this.MouseOverTooltip.SetToolTip(this.Button, newText);
            }
        }

        private static void _UpdateFocusTooltip()
        {
            new KeyboardEmulator().PressTab().HoldDownShift().PressTab();
        }
    }
}
