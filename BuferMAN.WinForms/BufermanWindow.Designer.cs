using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using magicmanam.Windows;
using BuferMAN.Assets;
using magicmanam.Windows.ClipboardViewer;
using BuferMAN.Infrastructure;
using System.ComponentModel;
using BuferMAN.Infrastructure.Window;
using BuferMAN.Clipboard;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.WinForms.Menu;
using Logging;
using BuferMAN.Infrastructure.Environment;
using BuferMAN.View;
using BuferMAN.Infrastructure.Settings;
using System.Windows.Input;
using System.Linq;

namespace BuferMAN.WinForms
{
    partial class BufermanWindow
    {
        private readonly IUserInteraction _userInteraction;
        private readonly IList<IBufer> _bufers;
        private ClipboardViewer _clipboardViewer;
        private readonly IRenderingHandler _renderingHandler;
        private NotifyIcon TrayIcon;
        private Label _userManualLabel;
        private Label _pinnedBufersDivider;// TODO (m) replace with Split Container (along with scrolling bufers feature and pinned area)
        private bool _isAdmin;
        private IBufermanApplication _bufermanApp;
        private bool _wasWindowClosed = false;
        private bool _wasWindowActivated = false;
        private IProgramSettingsGetter _settingsGetter;
        private IProgramSettingsSetter _settingsSetter;
        private static BufermanWindow _current;

        public INotificationEmitter NotificationEmitter { get; private set; }
        public event EventHandler ClipbordUpdated;
        public event EventHandler WindowActivated;
        public event EventHandler WindowHidden;

        public IEnumerable<IBufer> Bufers { get { return this._bufers.ToList(); } }
        internal StatusStrip StatusLine { get; set; }
        public ToolStripStatusLabel StatusLabel { get; set; }

        public BufermanWindow(
            IProgramSettingsGetter settingsGetter,
            IRenderingHandler renderingHandler,
            IUserInteraction userInteraction,
            IProgramSettingsSetter settingsSetter)
        {
            this._bufers = new List<IBufer>(settingsGetter.MaxBufersCount + settingsGetter.ExtraBufersCount);
            this._renderingHandler = renderingHandler;
            this._settingsGetter = settingsGetter;
            this._settingsSetter = settingsSetter;

            this._userInteraction = userInteraction;
            BufermanWindow._current = this;
        }

        public void SetMainMenu(IEnumerable<BufermanMenuItem> menuItems)
        {
            this.Menu = new MainMenu();
            this.Menu.PopulateMenuWithItems(menuItems);
        }

        public BufermanMenuItem CreateMenuItem(Func<string> textFn, EventHandler eventHandler = null)
        {
            return new FormMenuItem(textFn, eventHandler);
        }

        public void AddBufer(IBufer bufer)
        {
            this._bufers.Add(bufer);

            var button = bufer.GetButton();
            this.Controls.Add(button);
            button.BringToFront();
        }

        public void RemoveBufer(IBufer bufer)
        {
            this.Controls.Remove(bufer.GetButton());

            var wasRemoved = this._bufers.Remove(bufer);
            if (!wasRemoved)
            {
                throw new InvalidOperationException("There was an attempt to remove not existing bufer");
            }
        }

        public BufermanMenuItem CreateMenuItem(string text, EventHandler eventHandler = null)
        {
            return new FormMenuItem(text, eventHandler);
        }

        public BufermanMenuItem CreateMenuSeparatorItem()
        {
            return new FormMenuItem("-");
        }

        public void BuferFocused(object sender, BuferFocusedEventArgs e)
        {
            this._bufers
                .Single(b => b.ViewModel.ViewId == e.Bufer.ViewId)
                .GetButton()
                .Focus();
        }

        public void SetOnKeyDown(KeyEventHandler handler)
        {
            this.KeyDown += handler;
        }

        public IUserInteraction UserInteraction
        {
            get
            {
                return this._userInteraction;
            }
        }

        public void OnFullBuferMAN(object sender, EventArgs e)
        {
            this.UserInteraction.ShowPopup(Resource.AllBufersPinned, Resource.TratataTitle);
        }

        public bool IsVisible
        {
            get
            {
                return this.WindowState != FormWindowState.Minimized && this.Visible;
            }
        }

        public int InnerAreaWidth { get; private set; }

        public void Start(IBufermanApplication bufermanApp, bool isAdmin)
        {
            this._isAdmin = isAdmin;
            this._bufermanApp = bufermanApp;

            Application.ThreadException += BufermanWindow._Application_ThreadException;//Must be run before Application.Run() //Note

            Application.EnableVisualStyles();

            this.TrayIcon = new NotifyIcon()
            {
                Text = Resource.NotifyIconStartupText,
                Icon = Icons.Buferman,
                Visible = true
            };
            this.TrayIcon.DoubleClick += this._TrayIcon_DoubleClick;

            this.NotificationEmitter = new NotificationEmitter(this.TrayIcon, bufermanApp.GetBufermanTitle());
            this.NotificationEmitter.ShowInfoNotification(Resource.NotifyIconStartupText, 1500);

            this._InitializeForm();
            this.Text = isAdmin ?
                bufermanApp.GetBufermanAdminTitle() : 
                bufermanApp.GetBufermanTitle();

            this.InnerAreaWidth = this.ClientRectangle.Width;

            this._pinnedBufersDivider = new Label
            {
                Text = string.Empty,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSize = false,
                Height = 3,
                BackColor = Color.AliceBlue,
                Width = this.InnerAreaWidth
            };
            this.Controls.Add(this._pinnedBufersDivider);
            this._pinnedBufersDivider.BringToFront();

            bufermanApp.RunInHost(this);

            Application.Idle += (object sender, EventArgs args) =>
            {
                if (bufermanApp.NeedRerender)
                {
                   bufermanApp.ClearEmptyBufers();

                   bufermanApp.RerenderBufers();
                   bufermanApp.NeedRerender = false;
                }
            };

            Application.Run(this);
        }

        public void SetPinnedBufersDividerY(int y)
        {
            this._pinnedBufersDivider.Location = new Point(0, y);
        }

        public int PinnedBufersDividerHeight
        {
            get { return this._pinnedBufersDivider.Height; }
        }

        public void SuspendLayoutLogic()
        {
            this.SuspendLayout();
        }

        public void ResumeLayoutLogic()
        {
            this.ResumeLayout(false);
        }

        private void _StartTrickTimer(int intervalSeconds)
        {
            var trickTimer = new Timer();
            trickTimer.Interval = intervalSeconds * 1000;
            trickTimer.Tick += this._TrickTimer_Tick;
            trickTimer.Start();
        }//TODO (m) Find better solution

        private void _TrickTimer_Tick(object sender, EventArgs e)
        {
            this._clipboardViewer.RefreshViewer();
        }

        private IContainer _components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this._components != null))
            {
                this._components.Dispose();
                this.TrayIcon.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Messages.WM_CREATE)
            {
                this._clipboardViewer = new ClipboardViewer(this.Handle);
                WindowsFunctions.RegisterHotKey(this.Handle, 0, 1, (int)Keys.C);

                this._StartTrickTimer(23);
            }
            else if (this._clipboardViewer != null)
            {
                this._clipboardViewer.HandleWindowsMessage(m.Msg, m.WParam, m.LParam);
            }

            if (m.Msg == Messages.WM_DRAWCLIPBOARD)
            {
                this.ClipbordUpdated?.Invoke(this, EventArgs.Empty);
            }

            if (m.Msg == Messages.WM_HOTKEY)
            {
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                var modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                if (key == Keys.C && modifier == System.Windows.Input.ModifierKeys.Alt)
                {
                    this.Activate();
                }
            }

            if (m.Msg == Messages.WM_QUERYENDSESSION)
            {
                this._bufermanApp.Exit();
            }

            base.WndProc(ref m);
        }

        public void SetStatusBarText(string newText)
        {
            const int MAX_STATUS_LENGTH = 40;// TODO (m) Define based on window's width
            this.StatusLabel.ToolTipText = newText;
            this.StatusLabel.Text = newText.Length <= MAX_STATUS_LENGTH ? newText : newText.Substring(0, MAX_STATUS_LENGTH);

            this.StatusLine.Update();
        }

        public void ActivateWindow()
        {
            var escHotKeyIntroCounter = this._settingsGetter.EscHotKeyIntroductionCounter;

            if (!this._wasWindowActivated && escHotKeyIntroCounter < int.MaxValue / 8)
            {
                if (this._IsPowerOfTwo(escHotKeyIntroCounter))
                {
                    this.NotificationEmitter.ShowInfoNotification(Resource.EscHotKeyExplanation, 2500);
                    escHotKeyIntroCounter *= 4;
                }

                this._settingsSetter.EscHotKeyIntroductionCounter = escHotKeyIntroCounter - 1;

                this._wasWindowActivated = true;
            }

            this.WindowState = FormWindowState.Normal;
            this.Visible = true;
        }

        public void HideWindow()
        {
            this.WindowState = FormWindowState.Minimized;
            this.WindowHidden?.Invoke(this, new EventArgs());
        }

        public void RerenderBufers(IEnumerable<BuferViewModel> temporaryBuferViewModels, IEnumerable<BuferViewModel> pinnedBuferViewModels)
        {
            this._renderingHandler.Render(this, temporaryBuferViewModels, pinnedBuferViewModels);
        }

        public void RefreshUI(IEnumerable<BuferViewModel> temporaryBuferViewModels, IEnumerable<BuferViewModel> pinnedBuferViewModels)
        {
            this._bufermanApp.RerenderBufers();// TODO (l) this line will work only if I do not use cached button and will recreate all bufers !!!
            this.RerenderUserManual();
        }

        public Guid CurrentBuferViewId { get; set; }

        public void Exit()
        {
            this.TrayIcon.Visible = false;
            WindowsFunctions.UnregisterHotKey(this.Handle, 0);
            this._bufermanApp = null;

            Application.Exit();
        }

        public BuferViewModel LatestFocusedBufer { get; set; }

        private bool _IsPowerOfTwo(int number)
        {
            return (number & (number - 1)) == 0;
        }

        private void _InitializeForm()
        {
            this.SuspendLayout();

            this._components = new Container();

            this.ClientSize = new Size(290, 253);// Should be 290 because text must be located in status line
            this.DoubleBuffered = true;
            this.Icon = Icons.Buferman;

            this.ResumeLayout(false);// Should be before height assignment (or check an issue)

            this.AutoScaleMode = AutoScaleMode.Font;
            this.Height = 753 + 3 + 1;//+ is divider height + divider margin
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.WindowState = FormWindowState.Minimized;
            this._CreateStatusBar();
            this._CreateUserManualLabel();
            this._userManualLabel.Text = this._GetUserManualText();

            this.ShowInTaskbar = false;
            this.FormClosing += this._OnWindowClosing;
            this.Activated += this._onFormActivated;
        }

        private void _onFormActivated(object sender, EventArgs e)
        {
            this.WindowActivated?.Invoke(this, EventArgs.Empty);
        }

        public void SetTrayMenu(IEnumerable<BufermanMenuItem> menuItems)
        {
            var trayMenu = new System.Windows.Forms.ContextMenu();
            trayMenu.PopulateMenuWithItems(menuItems);

            this.TrayIcon.ContextMenu = trayMenu;
        }

        private void _TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Activate();
        }

        private void _CreateUserManualLabel()
        {
            this._userManualLabel = new Label() {
                ForeColor = Color.DarkGray,
                TabIndex = 1000,
                Height = 300,
                Width = 300,
                Parent = this
            };

            this._userManualLabel.Location = new Point(0, 430);
            this._userManualLabel.Padding = new Padding(10);
        }

        private string _GetUserManualText()
        {
            return this._isAdmin ?
                this._bufermanApp.GetUserManualText() :
                Resource.NotAdminWarning + this._bufermanApp.GetUserManualText();
        }

        public void RerenderUserManual()
        {
            this._userManualLabel.Text = this._GetUserManualText();
        }

        private void _CreateStatusBar()
        {
            this.StatusLine = new StatusStrip();
            this.StatusLabel = new ToolStripStatusLabel() {
                AutoToolTip = true,
                Alignment = ToolStripItemAlignment.Left,
                BorderSides = ToolStripStatusLabelBorderSides.Left,
                Margin = new Padding(3, 0, 0, 0)
            };

            this.StatusLine.SuspendLayout();
            this.SuspendLayout();

            this.StatusLine.Dock = DockStyle.Bottom;
            this.StatusLine.GripStyle = ToolStripGripStyle.Visible;
            this.StatusLine.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.StatusLine.ShowItemToolTips = true;
            this.StatusLine.SizingGrip = false;
            this.StatusLine.Stretch = false;
            this.StatusLine.TabIndex = 1000;

            this.StatusLine.Items.Add(this.StatusLabel);

            this.Controls.Add(this.StatusLine);

            this.StatusLine.ResumeLayout(false);
            this.StatusLine.PerformLayout();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        public void AddStatusLinePart(string text, Icon icon, EventHandler mouseEnterHandler)
        {
            var label = new ToolStripStatusLabel()
            {
                Spring = true,
                Alignment = ToolStripItemAlignment.Left,
                Image = icon?.ToBitmap(),
                Text = text
            };
            label.MouseEnter += (object sender, EventArgs e) =>
            {
                Action<string> statusLinePartAction = (tooltip) => label.ToolTipText = tooltip;
                mouseEnterHandler(statusLinePartAction, e);
            };

            this.StatusLine.Items.Insert(0, label);
        }

        private void _OnWindowClosing(object sender, FormClosingEventArgs e)
        {
            if (this._bufermanApp != null)
            {
                var closingWindowExplanationCounter = this._settingsGetter.ClosingWindowExplanationCounter;

                if (!this._wasWindowClosed && closingWindowExplanationCounter < int.MaxValue / 8)
                {
                    if (this._IsPowerOfTwo(closingWindowExplanationCounter))
                    {
                        this.NotificationEmitter.ShowInfoNotification(Resource.ClosingWindowExplanation, 2500);
                        closingWindowExplanationCounter *= 4;
                    }

                    this._settingsSetter.ClosingWindowExplanationCounter = closingWindowExplanationCounter - 1;

                    this._wasWindowClosed = true;
                }

                e.Cancel = true;
                this.HideWindow();
            }
        }

        private static void _Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Logger.WriteError("Exception " + e.Exception.Message, e.Exception);

            var exc = e.Exception as ClipboardMessageException;
            if (exc != null && BufermanWindow._current?.NotificationEmitter != null)
            {// Notification is used, because popup is blocking and UI is freezed with it
                BufermanWindow._current.NotificationEmitter.ShowErrorNotification(exc.Message, 0, exc.Title ?? Application.ProductName);
            }
        }
    }
}