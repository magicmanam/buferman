using Microsoft.Deployment.WindowsInstaller;
using System;
using System.IO;
using System.Windows.Forms;
using WixSharp;

namespace BuferMAN.Wix
{
    class Program
    {
        static void Main(string[] args)
        {
            var project = new ManagedProject("BuferMAN",
                          new Dir(@"%ProgramFiles%\magicmanam\BuferMAN",
                               new DirFiles(@"..\BuferMAN\bin\Release\*.dll"),
                               new DirFiles(@"..\BuferMAN\bin\Release\*.config"),
                               new DirFiles(@"..\BuferMAN\bin\Release\*.ico"),
                               new DirFiles(@"..\BuferMAN\bin\Release\*.xml"),
                               new DirFiles(@"..\BuferMAN\bin\Release\*.json"),
                               new DirFiles(@"..\BuferMAN\bin\Release\*.exe"),
                               new DirFiles(@"..\BuferMAN\bin\Release\*.html"),
                               new Dir("ru", new DirFiles(@"..\BuferMAN\bin\Release\ru\*.dll"))),
                          new Dir(@"%Desktop%",
                            new ExeFileShortcut("BuferMAN", Path.Combine("[INSTALLDIR]", "BuferMAN.exe"), arguments: "") {
                                WorkingDirectory = "[INSTALLDIR]",
                                IconFile = @"..\BuferMAN\bin\Release\buferman.ico"
                            }))
            {
                GUID = new Guid("564cedef-dfeb-4ad1-bfb4-2cd8388c28b9")
            };
            
            project.ControlPanelInfo.ProductIcon = @"..\BuferMAN\bin\Release\buferman.ico";
            project.ControlPanelInfo.Manufacturer = "Magicmanam";
            project.ControlPanelInfo.Contact = "Andrei Muski";
            project.ControlPanelInfo.Name = "BuferMAN";
            
            project.Version = System.Reflection.Assembly.LoadFrom(@"..\BuferMAN\bin\Release\BuferMAN.exe").GetName().Version;

            project.MajorUpgradeStrategy = MajorUpgradeStrategy.Default;

            project.AfterInstall += (SetupEventArgs e) =>
            {
                if (e.Result == ActionResult.Success)
                {
                    switch (e.Mode)
                    {
                        case SetupEventArgs.SetupMode.Installing:
                            MessageBox.Show(Resource.InstallComplete, Resource.InstallCompleteTitle);
                            break;
                        case SetupEventArgs.SetupMode.Uninstalling:
                            var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BuferMAN");
                            if (Directory.Exists(dataFolder))
                            {
                                Directory.Delete(dataFolder);
                            }
                            MessageBox.Show(Resource.UninstallComplete, Resource.InstallCompleteTitle);
                            break;
                    }
                }
            };

            project.UI = WUI.WixUI_ProgressOnly;

            Compiler.BuildMsi(project);
        }
    }
}
