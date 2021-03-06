﻿using BuferMAN.ContextMenu.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;
using magicmanam.Windows;
using BuferMAN.View;

namespace BuferMAN.ContextMenu
{
    public abstract class ChangingTextMenuItemBase : MenuItem
    {
        protected Button Button { get; private set; }
        protected ToolTip MouseOverTooltip { get; private set; }

        protected ChangingTextMenuItemBase(Button button, ToolTip mouseOverTooltip)
        {
            this.Button = button;
            this.MouseOverTooltip = mouseOverTooltip;
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
