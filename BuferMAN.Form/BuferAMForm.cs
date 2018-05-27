using System;
using SystemWindowsForm = System.Windows.Forms.Form;
using magicmanam.Windows;

namespace BuferMAN.Form
{
	public partial class BuferAMForm : SystemWindowsForm
    {
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            WindowsFunctions.UnregisterHotKey(this.Handle, 1);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BuferAMForm));
            this.SuspendLayout();
            // 
            // BuferAMForm
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BuferAMForm";
            this.ResumeLayout(false);

        }
    }
}
