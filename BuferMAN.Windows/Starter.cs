using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Environment;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace BuferMAN.Windows
{
    public class Starter : IStarter
    {
        private readonly IBufermanHost _bufermanHost;
        private readonly IBufermanApplication _bufermanApp;

        public Starter(IBufermanHost bufermanHost, IBufermanApplication bufermanApp)
        {
            this._bufermanHost = bufermanHost;
            this._bufermanApp = bufermanApp;
        }

        public void EnsureOneInstanceStart()
        {
            using (var mutex = new Mutex(false, "Global\\b5ebb574-5bac-4bc6-b18c-802a01f28ab2", out var isNew))
            {
                if (isNew)
                {
                    var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                    var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                    if (!isAdmin)
                    {
                        var result = this._bufermanHost.UserInteraction.ShowYesNoCancelPopup(Resource.AdminModeConfirmation, Resource.AdminModeConfirmationTitle);

                        switch (result)
                        {
                            case true:
                                this._bufermanHost.Start(this._bufermanApp, isAdmin);
                                return;
                            case false:
                                string arguments = "/select, \"" + Application.ExecutablePath + "\"";
                                Process.Start("explorer.exe", arguments).WaitForInputIdle();
                                return;
                            default:
                                return;
                        }
                    }

                    this._bufermanHost.Start(this._bufermanApp, isAdmin);
                }
                else
                {
                    this._bufermanHost.UserInteraction.ShowPopup(Resource.ProgramLaunched, Application.ProductName);// TODO (s) Application.ProductName ?
                }
            }
        }
    }
}
