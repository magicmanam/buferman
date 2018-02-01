using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Input;
using Windows;
using ClipboardViewerForm.Menu;
using ClipboardViewerForm.Window;
using ClipboardBufer;
using ClipboardViewerForm.Properties;

namespace ClipboardViewerForm
{
    partial class BuferAMForm
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        public const string PROGRAM_CAPTION = "BuferMAN";
        private readonly ICopyingToClipboardInterceptor _clipboardInterceptor;
        private readonly IMenuGenerator _menuGenerator;
        private readonly IEqualityComparer<IDataObject> _comparer;
        private readonly ILoadingFileHandler _loadingFileHandler;
        private readonly IDictionary<IDataObject, Button> _buttonsMap;
        private IntPtr _nextViewer;
        public const int MAX_BUFERS_COUNT = 30;

        internal StatusStrip StatusLine { get; set; }
        public ToolStripStatusLabel StatusLabel { get; set; }

        public BuferAMForm(IClipboardBuferService clipboardBuferService, IEqualityComparer<IDataObject> comparer, IClipboardWrapper clipboardWrapper, IProgramSettings settings)
        {
            InitializeComponent();
            InitializeForm();

            this._clipboardBuferService = clipboardBuferService;
            this._comparer = comparer;
            this._buttonsMap = new Dictionary<IDataObject, Button>(MAX_BUFERS_COUNT);
            this._clipboardInterceptor = new CopyingToClipboardInterceptor(clipboardBuferService, this, comparer, clipboardWrapper);
            this._loadingFileHandler = new LoadingFileHandler(clipboardWrapper);
            this._menuGenerator = new MenuGenerator(this._loadingFileHandler, this._clipboardBuferService, settings);
            this.Menu = this._menuGenerator.GenerateMenu();

            WindowLevelContext.SetCurrent(new DefaultWindowLevelContext(this, clipboardBuferService, comparer, clipboardWrapper, this._buttonsMap, settings));
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

        public void LoadBufersFromFile(string fileName)
        {
            this._loadingFileHandler.LoadBufersFromFile(fileName);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Messages.WM_CREATE)
            {
                this._nextViewer = WindowsFunctions.SetClipboardViewer(this.Handle);
                WindowsFunctions.RegisterHotKey(this.Handle, 0, 1, (int)Keys.C);
            }

            if (m.Msg == Messages.WM_DRAWCLIPBOARD)
            {
                this._clipboardInterceptor.DoOnCtrlC();

                this.SetStatusBarText(Resource.LastClipboardUpdate + DateTime.Now.ToShortTimeString());//Should be in separate strip label

                if (this._nextViewer != IntPtr.Zero)
                {
                    WindowsFunctions.SendMessage(this._nextViewer, m.Msg, IntPtr.Zero, IntPtr.Zero);
                }
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
                WindowsFunctions.ChangeClipboardChain(this.Handle, this._nextViewer);
                WindowsFunctions.UnregisterHotKey(this.Handle, 0);
                Application.Exit();//Note
            }

            if (m.Msg == Messages.WM_CHANGECBCHAIN)
            {
                if (this._nextViewer == m.WParam)
                {
                    this._nextViewer = m.LParam;
                }
                else
                {
                    WindowsFunctions.SendMessage(this._nextViewer, m.Msg, m.WParam, m.LParam);
                }
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

        private void InitializeForm()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = PROGRAM_CAPTION;
            this.Height = 753 + 3 + 1;//+ is divider height + divider margin
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.KeyDown += this._onKeyDown;
            this.KeyPreview = true;
            this.FormClosing += BuferAMForm_FormClosing;
            this.MaximizeBox = false;
            this.WindowState = FormWindowState.Minimized;
            this.CreateStatusBar();
            this.Activated += new WindowActivationHandler(this._clipboardBuferService, this).OnActivated;
            this.ShowInTaskbar = false;
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
            var form = (Form)sender;
            form.WindowState = FormWindowState.Minimized;
        }

        private void _onKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    WindowLevelContext.Current.HideWindow();
                    break;
                case Keys.Space:
                    new KeyboardEmulator().PressEnter();
                    break;
                case Keys.C:
                    new KeyboardEmulator().PressTab(3);
                    break;
                case Keys.X:
                    var lastBufer = this._clipboardBuferService.LastTemporaryClip;
                    if (lastBufer != null)
                    {
                        var button = this._buttonsMap.First(kv => this._comparer.Equals(lastBufer, kv.Key)).Value;
                        button.Focus();
                    }
                    break;
                case Keys.V:
                    var firstBufer = this._clipboardBuferService.FirstPersistentClip ?? this._clipboardBuferService.FirstTemporaryClip;

                    if (firstBufer != null)
                    {
                        var button = this._buttonsMap.First(kv => this._comparer.Equals(firstBufer, kv.Key)).Value;
                        button.Focus();
                    }
                    break;
            }
        }
    }
}

