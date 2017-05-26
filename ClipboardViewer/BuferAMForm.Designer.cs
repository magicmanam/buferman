using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Input;
using System.Linq;
using Windows;

namespace ClipboardViewer
{
    partial class BuferAMForm
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        internal const string PROGRAM_CAPTION = "BuferMAN";
        private readonly RenderingHandler _renderingHandler;
        private readonly WindowHidingHandler _hidingHandler;
        private readonly CopyingToClipboardInterceptor _clipboardInterceptor;
		private IntPtr _nextViewer;

		internal BuferAMForm(IClipboardBuferService clipboardBuferService, IEqualityComparer<IDataObject> comparer)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._hidingHandler = new WindowHidingHandler(this);
            this._renderingHandler = new RenderingHandler(this, this._clipboardBuferService, comparer, this._hidingHandler);
            this._clipboardInterceptor = new CopyingToClipboardInterceptor(clipboardBuferService, this, this._renderingHandler, comparer);
            
            InitializeComponent();
            this.ShowInTaskbar = false;
			Logger.Logger.Current.Write(this.Handle.ToString());
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
				this._nextViewer = WindowsFunctions.SetClipboardViewer(this.Handle);
				Logger.Logger.Current.Write("Next viewer " + this._nextViewer.ToString());
				WindowsFunctions.RegisterHotKey(this.Handle, 0, 1, (int)Keys.C);
			}

			if (m.Msg == Messages.WM_DRAWCLIPBOARD)
			{
				this._clipboardInterceptor.DoOnCtrlC();

				if (this._nextViewer != IntPtr.Zero)
				{
					Logger.Logger.Current.Write("Send message to the next clipboard viewer.");
					WindowsFunctions.SendMessage(this._nextViewer, m.Msg, IntPtr.Zero, IntPtr.Zero);
				}
			}

            if (m.Msg == Messages.WM_HOTKEY)
            {
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);   
                
                if(key == Keys.C && modifier == System.Windows.Input.ModifierKeys.Alt)
                {
					Logger.Logger.Current.Write("Activate window on Alt + C");
					this.Activate();
                }
            }

			if(m.Msg == Messages.WM_DESTROY)
			{
				WindowsFunctions.ChangeClipboardChain(this.Handle, this._nextViewer);
				WindowsFunctions.UnregisterHotKey(this.Handle, 0);
				Application.Exit();//Note
			}

			if(m.Msg == Messages.WM_CHANGECBCHAIN)
			{
				if(this._nextViewer == m.WParam)
				{
					this._nextViewer = m.LParam;
				} else
				{
					WindowsFunctions.SendMessage(this._nextViewer, m.Msg, m.WParam, m.LParam);
				}
			}

            base.WndProc(ref m);
        }

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
            
            this.KeyDown += this._renderingHandler.OnKeyDown;
            this.KeyPreview = true;

            var mainMenu = new MainMenu();
			mainMenu.MenuItems.Add(new MenuItem("File", new MenuItem[] { new MenuItem("Load from file"), new MenuItem("Exit session", OnExit) }));
            mainMenu.MenuItems.Add(new MenuItem("Edit", new MenuItem[] { new MenuItem("Undo", (sender, args) => MessageBox.Show("Feature is not supported now. Pay money to support.", "Keep calm and copy&paste!"), Shortcut.CtrlZ), new MenuItem("Delete All", OnDeleteAll), new MenuItem("Bufer's Basket", (sender, args) => MessageBox.Show("Available only in Free Pro version.", "Just copy&paste")) }));
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
			WindowsFunctions.SendMessage(this.Handle, Messages.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);			
        }

        private void OnDeleteAll(object sender, EventArgs args)
        {
			Logger.Logger.Current.Write("Delete All");

			var clips = this._clipboardBuferService.GetClips();

            foreach (var clip in clips)
            {
                this._clipboardBuferService.RemoveClip(clip);
            }

            this._renderingHandler.Render();
        }

        #endregion
    }
}

