using BuferMAN.Assets;
using BuferMAN.Infrastructure.Environment;
using BuferMAN.Infrastructure.Settings;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace BuferMAN.WinForms
{
    internal class BufermanOptionsWindow : Form, IBufermanOptionsWindow
    {
        private readonly IProgramSettingsGetter _settingsGetter;
        private readonly IProgramSettingsSetter _settingsSetter;
        private readonly IUserInteraction _userInteraction;
        private readonly IContainer _components = null;
        private readonly Button _saveButton;
        private readonly Button _restoreButton;
        private readonly Padding _flowLayoutPanelPadding = new Padding(3);
        private readonly Padding _groupBoxPadding = new Padding(5);

        public BufermanOptionsWindow(
            IProgramSettingsGetter settingsGetter,
            IProgramSettingsSetter settingsSetter,
            IUserInteraction userInteraction)
        {
            this._settingsGetter = settingsGetter;
            this._settingsSetter = settingsSetter;
            this._userInteraction = userInteraction;

            this.Icon = Icons.Buferman;
            this.TopMost = true;

            this.AutoSize = true;

            this._components = new Container();
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            this.Text = Resource.OptionsWindowTitle;
            this.Width = 425;

            this.SuspendLayout();

            var tabs = new TabControl()
            {
                Dock = DockStyle.Fill,
                Parent = this
            };

            tabs.TabPages.Add(this._CreateCommonTab());
            tabs.TabPages.Add(this._CreateUITab());
            //tabs.TabPages.Add(this._CreatePluginsTab());

            var controlPanel = new FlowLayoutPanel()
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Dock = DockStyle.Bottom,
                Parent = this
            };

            var cancelButton = new Button
            {
                Text = Resource.OptionsWindowCancelButton,
                AutoSize = true,
                Parent = controlPanel
            };
            cancelButton.Click += (object sender, EventArgs args) =>
            {
                this.Close();
            };
            this._saveButton = new Button {
                Text = Resource.OptionsWindowSaveButton,
                AutoSize = true,
                Parent = controlPanel,
                Enabled = false
            };
            this._saveButton.Click += (object sender, EventArgs args) =>
            {
                this._settingsSetter.Save();
                // TODO (s) rerender bufers because colors can be changed
                this.Close();
            };
            this._restoreButton = new Button
            {
                Text = Resource.OptionsWindowRestoreButton,
                AutoSize = true,
                Parent = controlPanel,
                Enabled = !this._settingsSetter.IsDefault
            };
            this._restoreButton.Click += (object sender, EventArgs args) =>
            {
                this._settingsSetter.RestoreDefault();
                this._settingsSetter.Save();
                // TODO (s) rerender bufers because colors can be changed
                this.Close();
            };

            this.ResumeLayout(false);
        }

        private TabPage _CreateCommonTab()
        {
            var commonTab = new TabPage()
            {
                Text = Resource.OptionsCommonTabName
            };

            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Parent = commonTab,
                Dock = DockStyle.Fill,
                Padding = this._flowLayoutPanelPadding
            };

            this._SetupStartupGroup(panel);
            this._SetupFocusTooltipGroup(panel);

            return commonTab;
        }

        private void _SetupFocusTooltipGroup(FlowLayoutPanel panel)
        {
            var tooltipGroupBox = new GroupBox
            {
                Text = Resource.OptionsFocusBuferTooltip,
                AutoSize = true,
                Margin = this._groupBoxPadding,
                Height = 0,
                Parent = panel
            };

            var onFocusTooltipGroupBoxPanel = BufermanOptionsWindow._CreateGroupBoxPanel(tooltipGroupBox);

            var focusTooltipCheckbox = new CheckBox
            {
                Text = Resource.OptionsFocusBuferEnableDisable,
                AutoSize = true,
                Checked = this._settingsGetter.ShowFocusTooltip,
                Parent = onFocusTooltipGroupBoxPanel
            };

            var tooltipDurationPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Parent = onFocusTooltipGroupBoxPanel,
                Margin = new Padding(0)
            };

            var tooltipDurationInput = new NumericUpDown()
            {
                AutoSize = true,
                Minimum = 1234,
                Maximum = 9876,
                Increment = 250,
                Enabled = this._settingsGetter.ShowFocusTooltip,
                Parent = tooltipDurationPanel
            };
            tooltipDurationInput.Value = this._settingsGetter.FocusTooltipDuration;

            focusTooltipCheckbox.CheckedChanged += (object sender, EventArgs args) =>
            {
                this._settingsSetter.ShowFocusTooltip = focusTooltipCheckbox.Checked;
                tooltipDurationInput.Enabled = focusTooltipCheckbox.Checked;

                this._saveButton.Enabled = this._settingsSetter.IsDirty;
                this._restoreButton.Enabled = !this._settingsSetter.IsDefault;
            };
            var tooltipDurationLabel = new Label
            {
                Text = Resource.OptionsFocusBuferTooltipDurationText,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom,
                Padding = new Padding(0, 5, 5, 5),
                Parent = tooltipDurationPanel
            };

            tooltipDurationInput.ValueChanged += (object sender, EventArgs args) =>
            {
                this._settingsSetter.FocusTooltipDuration = (int)tooltipDurationInput.Value;

                this._saveButton.Enabled = this._settingsSetter.IsDirty;
                this._restoreButton.Enabled = !this._settingsSetter.IsDefault;
            };
        }

        private void _SetupStartupGroup(FlowLayoutPanel panel)
        {
            var startupGroupBox = new GroupBox
            {
                Text = Resource.OptionsStartup,
                AutoSize = true,
                Margin = this._groupBoxPadding,
                Height = 0,
                Parent = panel
            };

            var startupGroupBoxPanel = BufermanOptionsWindow._CreateGroupBoxPanel(startupGroupBox);

            var showUserModeNotificationCheckbox = new CheckBox()
            {
                Text = Resource.OptionsAdministratorModeReminder,// TODO (s) rename this label
                Checked = this._settingsGetter.ShowUserModeNotification,
                AutoSize = true,
                Parent = startupGroupBoxPanel
            };
            var userModeTooltip = new ToolTip()
            {
                InitialDelay = 0,
                IsBalloon = true
            };
            userModeTooltip.SetToolTip(showUserModeNotificationCheckbox, Resource.OptionsAdministratorModeReminderTooltip);

            showUserModeNotificationCheckbox.CheckedChanged += (object sender, EventArgs args) =>
            {
                this._settingsSetter.ShowUserModeNotification = showUserModeNotificationCheckbox.Checked;

                this._saveButton.Enabled = this._settingsSetter.IsDirty;
                this._restoreButton.Enabled = !this._settingsSetter.IsDefault;
            };

            var restorePreviousSessionCheckbox = new CheckBox()
            {
                Text = Resource.OptionsRestorePreviousSession,
                Checked = this._settingsGetter.RestorePreviousSession,
                AutoSize = true,
                Parent = startupGroupBoxPanel
            };

            var restoreSessionTooltip = new ToolTip()
            {
                InitialDelay = 0,
                IsBalloon = true
            };
            restoreSessionTooltip.SetToolTip(restorePreviousSessionCheckbox, Resource.OptionsRestorePreviousSessionTooltip);

            restorePreviousSessionCheckbox.CheckedChanged += (object sender, EventArgs args) =>
            {
                this._settingsSetter.RestorePreviousSession = restorePreviousSessionCheckbox.Checked;

                this._saveButton.Enabled = this._settingsSetter.IsDirty;
                this._restoreButton.Enabled = !this._settingsSetter.IsDefault;
            };
        }

        private static FlowLayoutPanel _CreateGroupBoxPanel(GroupBox tooltipGroupBox)
        {
            return new FlowLayoutPanel()
            {
                FlowDirection = FlowDirection.TopDown,
                Top = 14,
                Left = 5,
                Height = 0,
                AutoSize = true,
                Parent = tooltipGroupBox
            };
        }

        private TabPage _CreateUITab()
        {
            var tab = new TabPage() { Text = Resource.OptionsUITabName };

            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Parent = tab
            };

            var buferButtonWidth = 250;
            var buferDefaultBackgroundColorButton = new Button
            {
                Text = Resource.OptionsChangeDefaultClipboardColor,
                Width = buferButtonWidth,
                BackColor = this._settingsGetter.BuferDefaultBackgroundColor,
                Parent = panel
            };
            buferDefaultBackgroundColorButton.Click += (object sender, EventArgs args) =>
            {
                var colorDialog = new ColorDialog
                {
                    Color = buferDefaultBackgroundColorButton.BackColor
                };

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    buferDefaultBackgroundColorButton.BackColor = colorDialog.Color;
                    this._settingsSetter.BuferDefaultBackgroundColor = colorDialog.Color;

                    this._saveButton.Enabled = this._settingsSetter.IsDirty;
                    this._restoreButton.Enabled = !this._settingsSetter.IsDefault;
                }
            };

            var pinnedBuferBackgroundColorButton = new Button
            {
                Text = Resource.OptionsChangePinnedClipboardColor,
                Width = buferButtonWidth,
                BackColor = this._settingsGetter.PinnedBuferBackgroundColor,
                Parent = panel
            };
            pinnedBuferBackgroundColorButton.Click += (object sender, EventArgs args) =>
            {
                var colorDialog = new ColorDialog
                {
                    Color = pinnedBuferBackgroundColorButton.BackColor
                };

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    pinnedBuferBackgroundColorButton.BackColor = colorDialog.Color;
                    this._settingsSetter.PinnedBuferBackgroundColor = colorDialog.Color;

                    this._saveButton.Enabled = this._settingsSetter.IsDirty;
                    this._restoreButton.Enabled = !this._settingsSetter.IsDefault;
                }
            };

            var currentBuferBackgroundColorButton = new Button
            {
                Text = Resource.OptionsChangeCurrentClipboardColor,
                Width = buferButtonWidth,
                BackColor = this._settingsGetter.CurrentBuferBackgroundColor,
                Parent = panel
            };
            currentBuferBackgroundColorButton.Click += (object sender, EventArgs args) =>
            {
                var colorDialog = new ColorDialog
                {
                    Color = currentBuferBackgroundColorButton.BackColor
                };

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    currentBuferBackgroundColorButton.BackColor = colorDialog.Color;
                    this._settingsSetter.CurrentBuferBackgroundColor = colorDialog.Color;

                    this._saveButton.Enabled = this._settingsSetter.IsDirty;
                    this._restoreButton.Enabled = !this._settingsSetter.IsDefault;
                }
            };

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
