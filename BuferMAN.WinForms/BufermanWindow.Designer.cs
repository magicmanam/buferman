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
using BuferMAN.Infrastructure.Files;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.WinForms.Menu;
using Logging;
using BuferMAN.Infrastructure.Environment;
using BuferMAN.View;
using BuferMAN.Infrastructure.Settings;
using System.Windows.Input;

namespace BuferMAN.WinForms
{
    partial class BufermanWindow
    {
        private readonly IUserInteraction _userInteraction;
        private readonly IDictionary<Guid, Button> _buttonsMap;
        private ClipboardViewer _clipboardViewer;
        private readonly IRenderingHandler _renderingHandler;
        private readonly IBufermanOptionsWindowFactory _optionsWindowFactory;
        private NotifyIcon TrayIcon;

        public INotificationEmitter NotificationEmitter { get; set; }
        public event EventHandler ClipbordUpdated;
        public event EventHandler WindowActivated;

        public IDictionary<Guid, Button> ButtonsMap { get { return this._buttonsMap; } }
        internal StatusStrip StatusLine { get; set; }
        public ToolStripStatusLabel StatusLabel { get; set; }

        public BufermanWindow(
            IProgramSettings settings,
            IClipboardBuferService clipboardBuferService,
            IFileStorage fileStorage,
            IRenderingHandler renderingHandler,
            IUserInteraction userInteraction,
            IBufermanOptionsWindowFactory optionsWindowFactory)
        {
            this._buttonsMap = new Dictionary<Guid, Button>(settings.MaxBufersCount + settings.ExtraBufersCount);
            this._renderingHandler = renderingHandler;

            this._userInteraction = userInteraction;
            this._optionsWindowFactory = optionsWindowFactory;
        }

        public void SetMainMenu(IEnumerable<BufermanMenuItem> menuItems)
        {
            this.Menu = new MainMenu();
            this.Menu.PopulateMenuWithItems(menuItems);
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
            this._buttonsMap[e.Bufer.ViewId].Focus();
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

        public void Start(IBufermanApplication bufermanApp, bool isAdmin)
        {
            Application.ThreadException += BufermanWindow._Application_ThreadException;//Must be run before Application.Run() //Note

            Application.EnableVisualStyles();

            this.TrayIcon = new NotifyIcon()
            {
                Text = Resource.NotifyIconStartupText,
                Icon = Icons.Buferman,
                Visible = true
            };

            this.NotificationEmitter = new NotificationEmitter(this.TrayIcon, Resource.WindowTitle);
            this.NotificationEmitter.ShowInfoNotification(Resource.NotifyIconStartupText, 1500);

            this._SetupTrayIcon();
            this._InitializeForm(isAdmin);

            this._renderingHandler.SetForm(this);

            bufermanApp.RunInHost(this);

            Application.Idle += this._Application_Idle;

            Application.Run(this);
        }

        private void _Application_Idle(object sender, EventArgs e)
        {
            
        }

        private void _StartTrickTimer(int intervalSeconds)
        {
            var trickTimer = new Timer();
            trickTimer.Interval = intervalSeconds * 1000;
            trickTimer.Tick += this._TrickTimer_Tick;
            trickTimer.Start();
        }//TODO Find better solution

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

            if (m.Msg == Messages.WM_DESTROY)
            {
                this.TrayIcon.Visible = false;
                WindowsFunctions.UnregisterHotKey(this.Handle, 0);
                Application.Exit();//Note
            }

            base.WndProc(ref m);
        }

        public void SetStatusBarText(string newText)
        {
            //this.StatusLabel.ToolTipText = newText;
            const int MAX_STATUS_LENGTH = 45;//Define based on window's width
            this.StatusLabel.Text = newText.Length <= MAX_STATUS_LENGTH ? newText : newText.Substring(0, MAX_STATUS_LENGTH);
            this.StatusLine.Update();
        }

        public void ActivateWindow()
        {
            this.WindowState = FormWindowState.Normal;
            this.Visible = true;
        }

        public void HideWindow()
        {
            this.WindowState = FormWindowState.Minimized;
        }

        public void RerenderBufers()
        {
            this._renderingHandler.Render(this);
        }

        public void SetCurrentBufer(BuferViewModel bufer)
        {
            this._renderingHandler.SetCurrentBufer(bufer);
        }

        public void Exit()
        {
            WindowsFunctions.SendMessage(this.Handle, Messages.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
        }

        private void _InitializeForm(bool isAdmin)
        {
            this.SuspendLayout();

            this._components = new Container();

            this.ClientSize = new Size(282, 253);
            this.DoubleBuffered = true;
            this.Icon = Icons.Buferman;

            this.AutoScaleMode = AutoScaleMode.Font;
            this.Text = isAdmin ? Resource.AdminWindowTitle : Resource.WindowTitle;
            this.Height = 753 + 3 + 1;//+ is divider height + divider margin
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.WindowState = FormWindowState.Minimized;
            this._CreateStatusBar();
            this._CreateUserManualLabel(isAdmin);

            this.ResumeLayout(false);

            this.ShowInTaskbar = false;
            this.FormClosing += this._OnWindowClosing;
            this.Activated += this._onFormActivated;
        }

        private void _onFormActivated(object sender, EventArgs e)
        {
            this.WindowActivated?.Invoke(this, EventArgs.Empty);
        }

        private void _SetupTrayIcon()
        {
            this.TrayIcon.DoubleClick += this._TrayIcon_DoubleClick;
            
            var trayMenu = new ContextMenu();
            var trayIconMenuItems = new List<BufermanMenuItem>();
            trayIconMenuItems.Add(this.CreateMenuItem(Resource.TrayMenuOptions, (object sender, EventArgs args) => this._optionsWindowFactory.Create().Open()));
            trayIconMenuItems.Add(this.CreateMenuItem(Resource.TrayMenuBuferManual, (object sernder, EventArgs args) => this.UserInteraction.ShowPopup(Resource.UserManual + Environment.NewLine + Environment.NewLine + Resource.DocumentationMentioning, Resource.WindowTitle)));
            trayIconMenuItems.Add(this.CreateMenuSeparatorItem());
            trayIconMenuItems.Add(this.CreateMenuItem(Resource.MenuFileExit, (object sender, EventArgs args) => this.Exit()));
            // TODO (s) into BufermanApplication
            trayMenu.PopulateMenuWithItems(trayIconMenuItems);
            this.TrayIcon.ContextMenu = trayMenu;
        }

        private void _TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Activate();
        }

        private void _CreateUserManualLabel(bool isAdmin)
        {
            var label = new Label() { ForeColor = Color.DarkGray, TabIndex = 1000, Height = 300, Width = 300 };
            label.Text = Resource.UserManual;

            if (!isAdmin)
            {
                label.Text = Resource.NotAdminWarning + Resource.UserManual;
            }

            label.Location = new Point(0, 430);
            label.Padding = new Padding(10);

            this.Controls.Add(label);
        }

        private void _CreateStatusBar()
        {
            StatusLine = new StatusStrip();
            StatusLabel = new ToolStripStatusLabel() { AutoToolTip = true, Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            StatusLine.SuspendLayout();
            SuspendLayout();

            StatusLine.Dock = DockStyle.Bottom;
            StatusLine.GripStyle = ToolStripGripStyle.Visible;
            StatusLine.Items.AddRange(new ToolStripItem[] { StatusLabel });
            StatusLine.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            StatusLine.ShowItemToolTips = true;
            StatusLine.SizingGrip = false;
            StatusLine.Stretch = false;
            StatusLine.TabIndex = 1000;

            Controls.Add(StatusLine);
            StatusLine.ResumeLayout(false);
            StatusLine.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private void _OnWindowClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            var form = (Form) sender;
            form.WindowState = FormWindowState.Minimized;
        }

        private static void _Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Logger.WriteError("Exception " + e.Exception.Message, e.Exception);

            var exc = e.Exception as ClipboardMessageException;
            if (exc != null)
            {
                MessageBox.Show(exc.Message, exc.Title ?? Application.ProductName);
            }
        }
    }
}