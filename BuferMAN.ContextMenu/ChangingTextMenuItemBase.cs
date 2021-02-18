using BuferMAN.ContextMenu.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;
using magicmanam.Windows;
using BuferMAN.View;

namespace BuferMAN.ContextMenu
{
    public abstract class ChangingTextMenuItemBase : MenuItem
    {
        protected string OriginBuferText { get; private set; }
        protected Button Button { get; private set; }
        protected ToolTip MouseOverTooltip { get; private set; }

        protected ChangingTextMenuItemBase(Button button, string originBuferText, ToolTip mouseOverTooltip)
        {
            this.Button = button;
            this.OriginBuferText = originBuferText;
            this.MouseOverTooltip = mouseOverTooltip;
        }

        public event EventHandler<TextChangedEventArgs> TextChanged;

        public void TryChangeText(string newText)
        {
            if (!string.IsNullOrWhiteSpace(newText) && newText != this.Button.Text)
            {
                this.Button.Text = newText;
                var buttonData = this.Button.Tag as BuferViewModel;
                buttonData.Representation = newText;
                ChangingTextMenuItemBase._UpdateFocusTooltip();

                bool isOriginText = newText == this.OriginBuferText;

                if (isOriginText)
                {
                    this.Button.Font = new Font(this.Button.Font, FontStyle.Regular);
                    MessageBox.Show(Resource.BuferAliasReturned, Resource.ChangeTextTitle);
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
