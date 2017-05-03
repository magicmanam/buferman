using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardViewer
{
    static class Program
    {        
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool isNew;
            using (var mutex = new Mutex(false, "Global\\b5ebb574-5bac-4bc6-b18c-802a01f28ab2", out isNew))
            {
                if (isNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    var form = new BuferAMForm(new ClipboardBuferService());
                    Application.Run(form);
                } else
                {                    
                    MessageBox.Show("Program is already run. Press Alt+C to view current bufers.", BuferAMForm.PROGRAM_CAPTION);
                }                
            }
        }        
    }
}
