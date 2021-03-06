﻿using BuferMAN.ContextMenu;
using BuferMAN.ContextMenu.Properties;
using BuferMAN.Infrastructure;
using Microsoft.VisualBasic;
using System;
using System.Windows.Forms;
using magicmanam.Windows;
using BuferMAN.View;

namespace ClipboardViewerForm.ClipMenu.Items
{
    public class CreateLoginCredentialsMenuItem : ChangingTextMenuItemBase
    {
        public CreateLoginCredentialsMenuItem(Button button, ToolTip mouseOverTooltip) : base(button, mouseOverTooltip)
        {
            this.Text = Resource.CreateCredsMenuItem;
            this.Click += this._CreateLoginCredentials;
            this.Shortcut = Shortcut.CtrlL;
        }

        public event EventHandler<CreateLoginCredentialsEventArgs> LoginCreated;

        private void _CreateLoginCredentials(object sender, EventArgs e)
        {
            var password = Interaction.InputBox(Resource.CreateCredsPrefix + $" \"{(this.Button.Tag as BuferViewModel).OriginBuferText}\". " + Resource.CreateCredsPostfix,
                  Resource.CreateCredsTitle,
                   null);

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(Resource.EmptyPasswordError, Resource.CreateCredsTitle);
            }
            else
            {
                this.Text = Resource.LoginCreds;
                this.LoginCreated?.Invoke(this, new CreateLoginCredentialsEventArgs(password));
                this.TryChangeText(Resource.CredsPrefix + $" {this.Button.Text}");

                this.Button.Click += (object pasteCredsSender, EventArgs args) =>
                {
                    WindowLevelContext.Current.HideWindow();

                    new KeyboardEmulator()
                        .TypeText((this.Button.Tag as BuferViewModel).OriginBuferText)
                        .PressTab()
                        .TypeText(password)
                        .PressEnter();
                };

                this.MenuItems.Add(new MenuItem(Resource.CredsPasswordEnter, (object pastePasswordSender, EventArgs args) =>
                {
                    WindowLevelContext.Current.HideWindow();
                    new KeyboardEmulator()
                        .TypeText(password)
                        .PressEnter();
                }));
                this.MenuItems.Add(new MenuItem(Resource.CredsPassword, (object pastePasswordSender, EventArgs args) =>
                {
                    WindowLevelContext.Current.HideWindow();

                    new KeyboardEmulator()
                        .TypeText(password);
                }));
                this.MenuItems.Add(new MenuItem(Resource.CredsName, (object pasteUsernameSender, EventArgs args) =>
                {
                    WindowLevelContext.Current.HideWindow();

                    new KeyboardEmulator()
                        .TypeText((this.Button.Tag as BuferViewModel).OriginBuferText);
                }));
            }
        }
    }
}
