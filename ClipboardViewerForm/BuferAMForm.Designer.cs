﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Input;
using System.Linq;
using Windows;
using System.IO;
using System.Text;
using ClipboardViewerForm.Menu;
using ClipboardViewerForm.Window;
using ClipboardBufer;
using Logging;

namespace ClipboardViewerForm
{
    partial class BuferAMForm
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        public const string PROGRAM_CAPTION = "BuferMAN";
        private readonly IRenderingHandler _renderingHandler;
        private readonly IWindowHidingHandler _hidingHandler;
        private readonly ICopyingToClipboardInterceptor _clipboardInterceptor;
        private readonly IMenuGenerator _menuGenerator;
        private IntPtr _nextViewer;

        internal StatusStrip StatusLine { get; set; }
        public ToolStripStatusLabel StatusLabel { get; set; }

        public BuferAMForm(IClipboardBuferService clipboardBuferService, IEqualityComparer<IDataObject> comparer, IClipboardWrapper clipboardWrapper)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._hidingHandler = new WindowHidingHandler(this);
            this._renderingHandler = new RenderingHandler(this, this._clipboardBuferService, comparer, this._hidingHandler, clipboardWrapper);
            this._clipboardInterceptor = new CopyingToClipboardInterceptor(clipboardBuferService, this, this._renderingHandler, comparer, clipboardWrapper);
            this._menuGenerator = new MenuGenerator(new LoadingFileHandler(clipboardWrapper), this._clipboardBuferService, this._renderingHandler);

            InitializeComponent();
            this.ShowInTaskbar = false;
            Logger.Write(this.Handle.ToString());
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
                WindowsFunctions.RegisterHotKey(this.Handle, 0, 1, (int)Keys.C);
            }

            if (m.Msg == Messages.WM_DRAWCLIPBOARD)
            {
                this._clipboardInterceptor.DoOnCtrlC();

                this.SetStatusBarText($"Clipboard last update was at {DateTime.Now.ToShortTimeString()}");//Should be in separate strip label

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
            this.Height = 832 + 3;//+ is divider height
            this.Activated += new WindowActivationHandler(_clipboardBuferService, this, this._renderingHandler).OnActivated;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            this.KeyDown += this._renderingHandler.OnKeyDown;
            this.KeyPreview = true;

            this.Menu = this._menuGenerator.GenerateMenu(this);

            this.FormClosing += BuferAMForm_FormClosing;

            this.MaximizeBox = false;

            this.WindowState = FormWindowState.Minimized;

            this.CreateStatusBar();
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

        #endregion
    }
}

