using BuferMAN.ContextMenu;
using BuferMAN.Infrastructure;
using System;
using System.Windows.Forms;
using magicmanam.Windows;
using BuferMAN.Infrastructure.Menu;

namespace ClipboardViewerForm.ClipMenu.Items
{
    internal class CreateLoginCredentialsMenuItem : ChangingTextMenuItemBase
    {
        public CreateLoginCredentialsMenuItem(BufermanMenuItem menuItem, IBufer bufer, IBufermanHost bufermanHost) : base(menuItem, bufer, bufermanHost)
        {
            menuItem.AddOnClickHandler(this._CreateLoginCredentials);
            menuItem.ShortCut = Shortcut.CtrlL;
        }

        public event EventHandler<CreateLoginCredentialsEventArgs> LoginCreated;

        private void _CreateLoginCredentials(object sender, EventArgs e)
        {
            var password = this.BufermanHost.UserInteraction.PromptPopup(Resource.CreateCredsPrefix + $" \"{this.Bufer.ViewModel.OriginBuferTitle}\". " + Resource.CreateCredsPostfix,
                  Resource.CreateCredsTitle,
                   null);

            if (string.IsNullOrWhiteSpace(password))
            {
                this.BufermanHost.UserInteraction.ShowPopup(Resource.EmptyPasswordError, Resource.CreateCredsTitle);
            }
            else
            {
                this.MenuItem.Text = Resource.LoginCreds;
                this.LoginCreated?.Invoke(this, new CreateLoginCredentialsEventArgs(password));
                this.TryChangeText(Resource.CredsPrefix + $" {this.Bufer.ViewModel.OriginBuferTitle}");

                this.Bufer.AddOnClickHandler((object pasteCredsSender, EventArgs args) =>
                {
                    this.BufermanHost.HideWindow();

                    new KeyboardEmulator()
                        .TypeText(this.Bufer.ViewModel.TextData)
                        .PressTab()
                        .TypeText(password)
                        .PressEnter();
                });

                var credsPasswordEnterMenuItem = this.BufermanHost.CreateMenuItem(Resource.CredsPasswordEnter, (object pastePasswordSender, EventArgs args) =>
                {
                    this.BufermanHost.HideWindow();
                    new KeyboardEmulator()
                        .TypeText(password)
                        .PressEnter();
                });
                this.MenuItem.AddMenuItem(credsPasswordEnterMenuItem);

                var credsPasswordMenuItem = this.BufermanHost.CreateMenuItem(Resource.CredsPassword, (object pastePasswordSender, EventArgs args) =>
                {
                    this.BufermanHost.HideWindow();

                    new KeyboardEmulator()
                        .TypeText(password);
                });
                this.MenuItem.AddMenuItem(credsPasswordMenuItem);

                var credsNameMenuItem = this.BufermanHost.CreateMenuItem(Resource.CredsName, (object pasteUsernameSender, EventArgs args) =>
                {
                    this.BufermanHost.HideWindow();

                    new KeyboardEmulator()
                        .TypeText(this.Bufer.ViewModel.TextData);
                });
                this.MenuItem.AddMenuItem(credsNameMenuItem);
            }
        }
    }
}
