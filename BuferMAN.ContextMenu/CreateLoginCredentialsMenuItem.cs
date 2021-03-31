﻿using BuferMAN.ContextMenu;
using BuferMAN.ContextMenu.Properties;
using BuferMAN.Infrastructure;
using Microsoft.VisualBasic;
using System;
using System.Windows.Forms;
using magicmanam.Windows;
using BuferMAN.View;
using BuferMAN.Infrastructure.Menu;

namespace ClipboardViewerForm.ClipMenu.Items
{
    public class CreateLoginCredentialsMenuItem : ChangingTextMenuItemBase
    {
        private readonly IBuferMANHost _buferMANHost;

        public CreateLoginCredentialsMenuItem(BuferMANMenuItem menuItem, Button button, ToolTip mouseOverTooltip, IBuferMANHost buferMANHost) : base(menuItem, button, mouseOverTooltip)
        {
            menuItem.SetOnClickHandler(this._CreateLoginCredentials);
            menuItem.ShortCut = Shortcut.CtrlL;
            this._buferMANHost = buferMANHost;
        }

        public event EventHandler<CreateLoginCredentialsEventArgs> LoginCreated;

        private void _CreateLoginCredentials(object sender, EventArgs e)
        {
            var password = Interaction.InputBox(Resource.CreateCredsPrefix + $" \"{(this.Button.Tag as BuferViewModel).OriginBuferText}\". " + Resource.CreateCredsPostfix,
                  Resource.CreateCredsTitle,
                   null);

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(Resource.EmptyPasswordError, Resource.CreateCredsTitle);// TODO -> buferMANHost method
            }
            else
            {
                this.MenuItem.Text = Resource.LoginCreds;
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

                var credsPasswordEnterMenuItem = this._buferMANHost.CreateMenuItem(Resource.CredsPasswordEnter, (object pastePasswordSender, EventArgs args) =>
                {
                    WindowLevelContext.Current.HideWindow();
                    new KeyboardEmulator()
                        .TypeText(password)
                        .PressEnter();
                });
                this.MenuItem.AddMenuItem(credsPasswordEnterMenuItem);

                var credsPasswordMenuItem = this._buferMANHost.CreateMenuItem(Resource.CredsPassword, (object pastePasswordSender, EventArgs args) =>
                {
                    WindowLevelContext.Current.HideWindow();

                    new KeyboardEmulator()
                        .TypeText(password);
                });
                this.MenuItem.AddMenuItem(credsPasswordMenuItem);

                var credsNameMenuItem = this._buferMANHost.CreateMenuItem(Resource.CredsName, (object pasteUsernameSender, EventArgs args) =>
                {
                    WindowLevelContext.Current.HideWindow();

                    new KeyboardEmulator()
                        .TypeText((this.Button.Tag as BuferViewModel).OriginBuferText);
                });
                this.MenuItem.AddMenuItem(credsNameMenuItem);
            }
        }
    }
}
