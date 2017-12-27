using log4net.Config;
using Logging;
using System;
using System.Threading;
using ClipboardBufer;
using System.Windows.Forms;
using ClipboardViewerForm;

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
					XmlConfigurator.Configure();//Note
					Logging.Logger.Current = new Log4netLogger();
					//Logger.Logger.Current = new ConsoleLogger();

					Application.ThreadException += Application_ThreadException;//Must be run before Application.Run() //Note

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
					var comparer = new DataObjectComparer(ClipboardFormats.StringFormats, ClipboardFormats.FileFormats);
                    var form = new BuferAMForm(new ClipboardBuferService(comparer), comparer, new ClipboardWrapper());
                    Application.Run(form);

				}
				else
				{
					MessageBox.Show("Program is already run. Press Alt+C to view current bufers.", BuferAMForm.PROGRAM_CAPTION);
				}
            }
        }

		private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			Logger.WriteError("Exception " + e.Exception.Message, e.Exception);
		}
	}
}
