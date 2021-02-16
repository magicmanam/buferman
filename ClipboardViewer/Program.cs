using log4net.Config;
using Logging;
using System;
using System.Threading;
using System.Windows.Forms;
using BuferMAN.Form;
using System.IO;
using System.Threading.Tasks;
using ClipboardViewer.Properties;
using System.Security.Principal;
using System.Diagnostics;
using magicmanam.UndoRedo;
using BuferMAN.Clipboard;
using BuferMAN.Settings;
using BuferMAN.Infrastructure;
using BuferMAN.Files;

namespace ClipboardViewer
{
	static class Program
    {
        private delegate void _LoadBufersFromDefaultFileInvoker();

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
                        var result = MessageBox.Show(Resource.AdminModeConfirmation, Resource.AdminModeConfirmationTitle, MessageBoxButtons.YesNoCancel);

                        switch (result)
                        {
                            case DialogResult.Yes:
                                Program._RunWindow();
                                return;
                            case DialogResult.No:
                                string arguments = "/select, \"" + Application.ExecutablePath + "\"";
                                Process.Start("explorer.exe", arguments).WaitForInputIdle();
                                return;
                            case DialogResult.Cancel:
                                return;
                        }
                    }

                    Program._RunWindow();
				}
				else
				{
					MessageBox.Show(Resource.ProgramLaunched, Application.ProductName);
				}
            }
        }

        private static void _RunWindow()
        {
            XmlConfigurator.Configure();//Note
            Logger.Current = new Log4netLogger();
            //Logger.Logger.Current = new ConsoleLogger();

            Application.ThreadException += Program._Application_ThreadException;//Must be run before Application.Run() //Note

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var comparer = new DataObjectComparer(ClipboardFormats.StringFormats, ClipboardFormats.FileFormats);
            var clipboardService = new ClipboardBuferService(comparer);
            var settings = new ProgramSettings();
            var clipboardWrapper = new ClipboardWrapper();
            UndoableContext<ClipboardBuferServiceState>.Current = new UndoableContext<ClipboardBuferServiceState>(clipboardService);
            var form = new BuferAMForm(clipboardService, comparer, clipboardWrapper, settings);
            WindowLevelContext.SetCurrent(new DefaultWindowLevelContext(form, clipboardService, comparer, clipboardWrapper, settings, new FileStorage()));

            Task.Delay(777).ContinueWith(t =>
            {
                if (File.Exists(settings.DefaultBufersFileName))
                {
                    var invoker = new _LoadBufersFromDefaultFileInvoker(form.LoadBufersFromStorage);
                    form.Invoke(invoker);
                }
            });

            UndoableContext<ClipboardBuferServiceState>.Current.UndoableAction += (object sender, UndoableActionEventArgs e) =>
            {
                form.SetStatusBarText(e.Action);
            };
            UndoableContext<ClipboardBuferServiceState>.Current.UndoAction += (object sender, UndoableActionEventArgs e) =>
            {
                form.SetStatusBarText(e.Action);
            };
            UndoableContext<ClipboardBuferServiceState>.Current.RedoAction += (object sender, UndoableActionEventArgs e) =>
            {
                form.SetStatusBarText(e.Action);
            };
            Application.Run(form);
        }

        private static void _Application_ThreadException(object sender, ThreadExceptionEventArgs e)
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
