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
using BuferMAN.Infrastructure.Window;
using BuferMAN.ContextMenu;
using BuferMAN.Infrastructure.ContextMenu;

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

            Program.Container = new Container();// TODO into separate project
            Program.Container.Register<IProgramSettings, ProgramSettings>(Lifestyle.Singleton);
            Program.Container.Register<IClipboardWrapper, ClipboardWrapper>(Lifestyle.Singleton);
            Program.Container.Register<IEqualityComparer<IDataObject>>(() => new DataObjectComparer(ClipboardFormats.StringFormats, ClipboardFormats.FileFormats), Lifestyle.Singleton);
            Program.Container.Register<IClipboardBuferService, ClipboardBuferService>(Lifestyle.Singleton);
            Program.Container.Register<IBufersFileParser, JsonFileParser>(Lifestyle.Singleton);
            Program.Container.Register<IIDataObjectHandler, DataObjectHandler>(Lifestyle.Singleton);
            Program.Container.Register<ILoadingFileHandler, LoadingFileHandler>(Lifestyle.Singleton);
            Program.Container.Register<IFileStorage, FileStorage>(Lifestyle.Singleton);
            Program.Container.Register<IBuferMANHost, BuferAMForm>(Lifestyle.Singleton);
            Program.Container.Register<IClipMenuGenerator, ClipMenuGenerator>(Lifestyle.Singleton);
            Program.Container.Register<IBuferSelectionHandlerFactory, BuferSelectionHandlerFactory>(Lifestyle.Singleton);
            Program.Container.Register<BuferMANApplication>(Lifestyle.Singleton);
            Program.Container.Register<IMenuGenerator, MenuGenerator>(Lifestyle.Singleton);
            Program.Container.Register<IWindowLevelContext, DefaultWindowLevelContext>(Lifestyle.Singleton);
            Program.Container.Register<IRenderingHandler, RenderingHandler>(Lifestyle.Singleton);

            Program.Container.Verify();

            var app = Program.Container.GetInstance<BuferMANApplication>();
        }
	}
}
