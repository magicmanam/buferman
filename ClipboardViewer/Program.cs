using log4net.Config;
using Logging;
using System;
using System.Threading;
using System.Windows.Forms;
using BuferMAN.Form;
using BuferMAN.Application;
using BuferMAN.Menu;
using ClipboardViewer.Properties;
using System.Security.Principal;
using System.Diagnostics;
using BuferMAN.Clipboard;
using BuferMAN.Settings;
using BuferMAN.Infrastructure;
using BuferMAN.Files;
using SimpleInjector;
using System.Collections.Generic;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Form.Window;
using BuferMAN.Infrastructure.Menu;

namespace ClipboardViewer
{
	static class Program
    {
        static Container Container;

        [STAThread]
        static void Main()
        {
            using (var mutex = new Mutex(false, "Global\\b5ebb574-5bac-4bc6-b18c-802a01f28ab2", out var isNew))
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

            Program.Container = new Container();
            Program.Container.Register<IProgramSettings, ProgramSettings>(Lifestyle.Singleton);
            Program.Container.Register<IClipboardWrapper, ClipboardWrapper>(Lifestyle.Singleton);
            Program.Container.Register<IEqualityComparer<IDataObject>>(() => new DataObjectComparer(ClipboardFormats.StringFormats, ClipboardFormats.FileFormats), Lifestyle.Singleton);
            Program.Container.Register<IClipboardBuferService, ClipboardBuferService>(Lifestyle.Singleton);
            Program.Container.Register<IBufersFileParser, JsonFileParser>(Lifestyle.Singleton);
            Program.Container.Register<IIDataObjectHandler, DataObjectHandler>(Lifestyle.Singleton);
            Program.Container.Register<ILoadingFileHandler, LoadingFileHandler>(Lifestyle.Singleton);
            Program.Container.Register<IFileStorage, FileStorage>(Lifestyle.Singleton);
            Program.Container.Register<IBuferMANHost, BuferAMForm>(Lifestyle.Singleton);
            Program.Container.Register<BuferMANApplication>(Lifestyle.Singleton);
            Program.Container.Register<IMenuGenerator, MenuGenerator>(Lifestyle.Singleton);

            Program.Container.Verify();

            var comparer = Program.Container.GetInstance<IEqualityComparer<IDataObject>>();
            var clipboardService = Program.Container.GetInstance<IClipboardBuferService>();
            var settings = Program.Container.GetInstance<IProgramSettings>();
            var clipboardWrapper = Program.Container.GetInstance<IClipboardWrapper>();

            var parser = Program.Container.GetInstance<IBufersFileParser>();

            var app = Program.Container.GetInstance<BuferMANApplication>();
            var form = Program.Container.GetInstance<IBuferMANHost>() as BuferAMForm;

            var dataObjectHandler = Program.Container.GetInstance<IIDataObjectHandler>();
            var loadingFileHandler = Program.Container.GetInstance<ILoadingFileHandler>();

            app.BuferFocused += form.BuferFocused;
            form.KeyDown += app.OnKeyDown;
            // TODO remove this 2 creations
            var renderingHandler = new RenderingHandler(form, clipboardService, comparer, clipboardWrapper, settings, Program.Container.GetInstance<IFileStorage>());
            WindowLevelContext.SetCurrent(new DefaultWindowLevelContext(form, renderingHandler));

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
