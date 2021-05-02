using System;
using System.Drawing;
using magicmanam.Windows;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure;

namespace BuferMAN.ContextMenu
{
    public abstract class ChangingTextMenuItemBase
    {
        protected IBufermanHost BufermanHost { get; private set; }

        protected BufermanMenuItem MenuItem { get; private set; }

        protected IBufer Bufer { get; private set; }

        protected ChangingTextMenuItemBase(BufermanMenuItem menuItem, IBufer bufer, IBufermanHost bufermanHost)
        {
            this.MenuItem = menuItem;
            this.Bufer = bufer;
            this.BufermanHost = bufermanHost;
        }

        public event EventHandler<TextChangedEventArgs> TextChanged;

        public void TryChangeText(string newText)
        {
            if (!string.IsNullOrWhiteSpace(newText) && newText != this.Bufer.Text)
            {
                this.Bufer.Text = newText;
                this.Bufer.ViewModel.Representation = newText;
                ChangingTextMenuItemBase._UpdateFocusTooltip();

                bool isOriginText = newText == this.Bufer.ViewModel.OriginBuferText;

                if (isOriginText)
                {
                    this.Bufer.ApplyFontStyle(FontStyle.Regular);
                    this.BufermanHost.UserInteraction.ShowPopup(Resource.BuferAliasReturned, Resource.ChangeTextTitle);
                }
                else
                {
                    this.Bufer.ApplyFontStyle(FontStyle.Bold);
                }

                this.TextChanged?.Invoke(this, new TextChangedEventArgs(isOriginText));
                this.Bufer.SetMouseOverToolTip(newText);
            }
        }

        private static void _UpdateFocusTooltip()
        {
            new KeyboardEmulator().PressTab().HoldDownShift().PressTab();
        }
    }
}
