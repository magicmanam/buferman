using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Input;
using System.Linq;

namespace ClipboardViewer
{
    partial class BuferAMForm
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        internal const string PROGRAM_CAPTION = "BuferMAN";
        private readonly RenderingHandler _renderingHandler;
        private readonly WindowHidingHandler _hidingHandler;
        private readonly CopyingToClipboardInterceptor _clipboardInterceptor;

        internal BuferAMForm(IClipboardBuferService clipboardBuferService)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._renderingHandler = new RenderingHandler(this, this._clipboardBuferService);
            this._hidingHandler = new WindowHidingHandler(this);
            this._clipboardInterceptor = new CopyingToClipboardInterceptor(clipboardBuferService, this, this._renderingHandler);
            
            InitializeComponent();
            this.ShowInTaskbar = false;
            
            this._nextViewer = SetClipboardViewer(this.Handle);

            RegisterHotKey(this.Handle, 0, 1, (int)Keys.C);
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

        private static int WM_HOTKEY = 0x0312;
        private static int WM_DRAWCLIPBOARD = 0x308;
        private string _lastText;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_DRAWCLIPBOARD)
            {
                this._clipboardInterceptor.DoOnCtrlC();
            }

            if (m.Msg == WM_HOTKEY)
            {
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);   
                
                if(key == Keys.C && modifier == System.Windows.Input.ModifierKeys.Alt)
                {               
                    this.Activate();
                }
            }

            base.WndProc(ref m);
        }
                
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = PROGRAM_CAPTION;
            this.Height = 807;
            this.Activated += new WindowActivationHandler(_clipboardBuferService, this, this._renderingHandler).OnActivated;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            
            this.KeyDown += BuferAMForm_KeyDown;
            this.KeyPreview = true;

            var mainMenu = new MainMenu();
            mainMenu.MenuItems.Add(new MenuItem("Actions", new MenuItem[] { new MenuItem("Undo", (sender, args) => MessageBox.Show("Feature is not supported now. Pay money to support.", "Keep calm and copy&paste!"), Shortcut.CtrlZ), new MenuItem("Delete All", OnDeleteAll), new MenuItem("Bufer's Basket", (sender, args) => MessageBox.Show("Available only in Free Pro version.", "Just copy&paste")), new MenuItem("Exit session", OnExit) }));
            this.Menu = mainMenu;

            this.FormClosing += BuferAMForm_FormClosing;

            this.MaximizeBox = false;

            this.WindowState = FormWindowState.Minimized;            
        }

        private void BuferAMForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            var form = (Form)sender;
            form.WindowState = FormWindowState.Minimized;
        }
        
        private void OnExit(object sender, EventArgs args)
        {
            System.Environment.Exit(0);
        }

        private void OnDeleteAll(object sender, EventArgs args)
        {
            var clips = this._clipboardBuferService.GetClips();

            foreach (var clip in clips)
            {
                this._clipboardBuferService.RemoveClip(clip);
            }

            this._renderingHandler.Render();
        }
        
        private void BuferAMForm_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
            {
                this._hidingHandler.HideWindow();
            }
        }

        #endregion
    }
}

