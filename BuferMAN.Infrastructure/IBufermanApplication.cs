using BuferMAN.Infrastructure.Menu;
using System.Collections.Generic;

namespace BuferMAN.Infrastructure
{
    public interface IBufermanApplication
    {
        void RunInHost(IBufermanHost bufermanHost);
        bool ShouldCatchCopies { get; set; }
        IBufermanHost Host { get; }
        IEnumerable<BufermanMenuItem> GetTrayMenuItems();
        bool NeedRerender { get; set; }
        string GetBufermanTitle();
        string GetBufermanAdminTitle();
        string GetUserManualText();
        void Exit();
        void RefreshMainMenu();
        void ChangeLanguage(string shortLanguage);

        /// <summary>
        /// Clear bufers that do not contain any data (operation system releases data objects sometimes).
        /// </summary>
        void ClearEmptyBufers();
        void RerenderBufers(BufersFilter bufersFilter = null);
    }
}
