using System;
using System.Drawing;
using magicmanam.Windows;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure;
using BuferMAN.View;

namespace BuferMAN.ContextMenu
{
    public abstract class ChangingTextMenuItemBase
    {
        protected IBufermanHost BufermanHost { get; private set; }

        protected BufermanMenuItem MenuItem { get; private set; }

        protected BuferViewModel ViewModel { get; private set; }

        protected IBufer Bufer { get; private set; }

        protected ChangingTextMenuItemBase(BufermanMenuItem menuItem, IBufer bufer, IBufermanHost bufermanHost)
        {
            this.MenuItem = menuItem;
            this.Bufer = bufer;
            this.ViewModel = bufer.ViewModel;
            this.BufermanHost = bufermanHost;
        }

        public event EventHandler<TextChangedEventArgs> TextChanged;

        public void TryChangeText(string newText)
        {
            if (!string.IsNullOrWhiteSpace(newText) && newText != this.ViewModel.Alias)
            {
                this.Bufer.SetText(newText);// TODO (l) remove this method void SetText(...) and add rebind/rendering
                this.ViewModel.Representation = newText;
                this.ViewModel.TextRepresentation = newText;
                ChangingTextMenuItemBase._UpdateFocusTooltip();

                bool isOriginText = newText == this.ViewModel.OriginBuferTitle;

                if (isOriginText)
                {
                    this.ViewModel.Alias = null;
                    this.Bufer.ApplyFontStyle(FontStyle.Regular);
                    this.BufermanHost.UserInteraction.ShowPopup(Resource.BuferAliasReturned, Resource.ChangeTextTitle);
                }
                else
                {
                    this.ViewModel.Alias = newText;
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
