using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using SystemWindowsFormsContextMenu = System.Windows.Forms.ContextMenu;
using magicmanam.Windows;
using System.Security.Principal;
using magicmanam.Windows.ClipboardViewer;
using BuferMAN.Infrastructure;
using SystemWindowsForm = System.Windows.Forms.Form;
using BuferMAN.Form.Properties;
using System.Windows.Input;
using BuferMAN.Application;
using System.ComponentModel;
using BuferMAN.Infrastructure.Window;
using BuferMAN.Form.Window;
using BuferMAN.Clipboard;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Infrastructure.Menu;

namespace BuferMAN.Form
{
    partial class BuferAMForm
    {
        private readonly IEqualityComparer<IDataObject> _comparer;
        private readonly IDictionary<Guid, Button> _buttonsMap;
        private ClipboardViewer _clipboardViewer;
        private readonly IRenderingHandler _renderingHandler;
        private readonly IMenuGenerator _menuGenerator;
        private NotifyIcon TrayIcon;

        public INotificationEmitter NotificationEmitter { get; set; }
        public event EventHandler ClipbordUpdated;
        public event EventHandler WindowActivated;

        public IDictionary<Guid, Button> ButtonsMap { get { return this._buttonsMap; } }
        internal StatusStrip StatusLine { get; set; }
        public ToolStripStatusLabel StatusLabel { get; set; }

        public BuferAMForm(IEqualityComparer<IDataObject> comparer, IProgramSettings settings, IClipboardBuferService clipboardBuferService, IClipboardWrapper clipboardWrapper, IFileStorage fileStorage, IMenuGenerator menuGenerator)
        {
            InitializeComponent();
            InitializeForm();

            this.NotificationEmitter = new NotificationEmitter(this.TrayIcon, Resource.WindowTitle);
            this._comparer = comparer;
            this._buttonsMap = new Dictionary<Guid, Button>(settings.MaxBufersCount + settings.ExtraBufersCount);

            this._StartTrickTimer(23);
            this.NotificationEmitter.ShowInfoNotification(Resource.NotifyIconStartupText, 1500);

            this._renderingHandler = new RenderingHandler(this, clipboardBuferService, comparer, clipboardWrapper, settings, fileStorage);
            this._menuGenerator = menuGenerator;
        }

        public void GenerateMenu()
        {
            this.Menu = this._menuGenerator.GenerateMenu(this);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(BuferAMForm));
            this.SuspendLayout();

            this.ClientSize = new Size(282, 253);
            this.DoubleBuffered = true;
            this.Icon = resources.GetObject("$this.Icon") as Icon;
            this.ResumeLayout(false);
        }

        public void BuferFocused(object sender, BuferFocusedEventArgs e)
        {
            this._buttonsMap[e.Bufer.ViewId].Focus();
        }

        public void OnFullBuferMAN(object sender, EventArgs e)
        {
            MessageBox.Show(Resource.AllBufersPinned, Resource.TratataTitle);
            // Maybe display a program window if not ?
        }

        public bool IsVisible
        {
            get
            {
                return this.WindowState != FormWindowState.Minimized && this.Visible;
            }
        }

        //TODO Find better solution
        private void _StartTrickTimer(int intervalSeconds)
        {
            var trickTimer = new Timer();
            trickTimer.Interval = intervalSeconds * 1000;
            trickTimer.Tick += this._TrickTimer_Tick;
            trickTimer.Start();
        }

        private void _TrickTimer_Tick(object sender, EventArgs e)
        {
            this._clipboardViewer.RefreshViewer();
        }

        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Messages.WM_CREATE)
            {
                this._clipboardViewer = new ClipboardViewer(this.Handle);
                WindowsFunctions.RegisterHotKey(this.Handle, 0, 1, (int)Keys.C);
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
                ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                if (key == Keys.C && modifier == System.Windows.Input.ModifierKeys.Alt)
                {
                    this.Activate();
                }
            }

            if (m.Msg == Messages.WM_DESTROY)
            {
                this.TrayIcon.Visible = false;
                WindowsFunctions.UnregisterHotKey(this.Handle, 0);
                System.Windows.Forms.Application.Exit();//Note
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
            this._renderingHandler.Render();
        }

        public void Exit()
        {
            WindowsFunctions.SendMessage(this.Handle, Messages.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
        }

        private void InitializeForm()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = Resource.WindowTitle;
            this.Height = 753 + 3 + 1;//+ is divider height + divider margin
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.FormClosing += BuferAMForm_FormClosing;
            this.MaximizeBox = false;
            this.WindowState = FormWindowState.Minimized;
            this.CreateStatusBar();
            this.Activated += this._onFormActivated;
            this.ShowInTaskbar = false;
            this._CreateUserManualLabel();
            this._SetupTrayIcon();
        }

        private void _onFormActivated(object sender, EventArgs e)
        {
            this.WindowActivated?.Invoke(this, EventArgs.Empty);
        }

        private void _SetupTrayIcon()
        {
            this.TrayIcon = new NotifyIcon() { Text = Resource.NotifyIconStartupText, Icon = new Icon("copy-multi-size.ico") };
            this.TrayIcon.DoubleClick += this._TrayIcon_DoubleClick;
            this.TrayIcon.ContextMenu = new SystemWindowsFormsContextMenu();
            this.TrayIcon.ContextMenu.MenuItems.Add(new MenuItem(Resource.MenuFileExit, (object sender, EventArgs args) => this.Exit()));
            this.TrayIcon.Visible = true;
        }

        private void _TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Activate();
        }

        private void _CreateUserManualLabel()
        {
            var label = new Label() { ForeColor = Color.DarkGray, TabIndex = 1000, Height = 300, Width = 300 };
            label.Text = Resource.UserManual;
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                label.Text = Resource.NotAdminWarning + Resource.UserManual;
            }

            label.Location = new Point(0, 430);
            label.Padding = new Padding(10);

            this.Controls.Add(label);
        }

        private void CreateStatusBar()
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

        private void BuferAMForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            var form = (SystemWindowsForm)sender;
            form.WindowState = FormWindowState.Minimized;
        }
    }
}

