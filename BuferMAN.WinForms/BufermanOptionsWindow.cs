using BuferMAN.Assets;
using BuferMAN.Infrastructure.Settings;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BuferMAN.WinForms
{
    internal class BufermanOptionsWindow : Form, IBufermanOptionsWindow
    {
        private readonly IProgramSettings _settings;
        private readonly IContainer _components = null;

        public BufermanOptionsWindow(IProgramSettings settings)
        {
            this._settings = settings;

            this.Icon = Icons.Buferman;
            this.TopMost = true;

            this.Size = new Size(300, 300);

            this._components = new Container();
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            this.Text = Resource.OptionsWindowTitle;

            var tabs = new TabControl() { Size = new Size(300, 200) };

            tabs.TabPages.Add(this._CreateCommonTab());
            //tabs.TabPages.Add(this._CreateUITab());
            tabs.TabPages.Add(this._CreatePluginsTab());

            this.Controls.Add(tabs);
        }

        private TabPage _CreateCommonTab()
        {
            var commonTab = new TabPage() { Text = Resource.OptionsCommonTabName };

            var group = new GroupBox();
            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown
            };

            var showUserModeNotificationCheckbox = new CheckBox()
            {
                Text = "Show user mode notification on BuferMAN startup",
                Checked = this._settings.ShowUserModeNotification,
                Top = 30,
                Left = 30,
                Width = 400
            };
            panel.Controls.Add(showUserModeNotificationCheckbox);

            var tooltipDurationInput = new TextBox { Text = "Bufer's tooltip duration", Size = new Size(50, 50), Location = new Point(100, 100) };
            panel.Controls.Add(tooltipDurationInput);

            group.Controls.Add(panel);  
            
            var uiGroup = new GroupBox();

            commonTab.Controls.Add(group);
            commonTab.Controls.Add(uiGroup);

            return commonTab;
        }

        private TabPage _CreateUITab()
        {
            var tab = new TabPage() { Text = Resource.OptionsUITabName };
            
            return tab;
        }

        private TabPage _CreatePluginsTab()
        {
            var tab = new TabPage() { Text = Resource.OptionsPluginsTabName };

            return tab;
        }

        public void Open()
        {
            this.Show();
        }
    }
}
