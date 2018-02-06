using log4net.Config;
using Logging;
using System;
using System.Threading;
using ClipboardBufer;
using System.Windows.Forms;
using ClipboardViewerForm;
using System.IO;
using System.Threading.Tasks;
using ClipboardViewer.Properties;
using System.Security.Principal;
using System.Diagnostics;

namespace ClipboardViewer
{
	static class Program
    {
        private delegate void _LoadBufersFromDefaultFileInvoker(string fileName);

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
                    WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                    if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                    {
                        var result = MessageBox.Show(Resource.AdminModeConfirmation, Resource.AdminModeConfirmationTitle, MessageBoxButtons.OKCancel);
                        if (result == DialogResult.Cancel)
                        {
                            string arguments = "/select, \"" + Application.ExecutablePath + "\"";
                            Process.Start("explorer.exe", arguments).WaitForInputIdle();
                            return;
                        }
                    }

                    XmlConfigurator.Configure();//Note
					Logger.Current = new Log4netLogger();
					//Logger.Logger.Current = new ConsoleLogger();

					Application.ThreadException += Application_ThreadException;//Must be run before Application.Run() //Note

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
					var comparer = new DataObjectComparer(ClipboardFormats.StringFormats, ClipboardFormats.FileFormats);
                    var clipboardService = new ClipboardBuferService(comparer);
                    var settings = new ProgramSettings();
                    var form = new BuferAMForm(clipboardService, comparer, new ClipboardWrapper(), settings);

                    Task.Delay(777).ContinueWith(t =>
                    {
                        if (File.Exists(settings.DefaultBufersFileName))
                        {
                            var invoker = new _LoadBufersFromDefaultFileInvoker(form.LoadBufersFromFile);
                            form.Invoke(invoker, settings.DefaultBufersFileName);
                        }
                    });

                    clipboardService.UndoableAction += (object sender, UndoableActionEventArgs e) =>
                    {
                        form.SetStatusBarText(e.Action);
                    };
                    clipboardService.UndoAction += (object sender, UndoableActionEventArgs e) =>
                    {
                        form.SetStatusBarText(e.Action);
                    };
                    clipboardService.CancelUndoAction += (object sender, UndoableActionEventArgs e) =>
                    {
                        form.SetStatusBarText(e.Action);
                    };
                    Application.Run(form);
				}
				else
				{
					MessageBox.Show(Resource.ProgramLaunched, BuferAMForm.PROGRAM_CAPTION);
				}
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			Logger.WriteError("Exception " + e.Exception.Message, e.Exception);
		}
	}
}
