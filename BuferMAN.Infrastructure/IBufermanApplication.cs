using BuferMAN.Infrastructure.Menu;
using System.Collections.Generic;

namespace BuferMAN.Infrastructure
{
    public interface IBufermanApplication
    {
        void SetHost(IBufermanHost bufermanHost);
        void Run();
        bool ShouldCatchCopies { get; set; }
        IBufermanHost Host { get; }
        IEnumerable<BufermanMenuItem> GetTrayMenuItems();
        bool NeedRerender { get; set; }
        string GetBufermanTitle();
        string GetBufermanAdminTitle();
        string GetUserManualText();
        void RefreshMainMenu();

        /// <summary>
        /// Clear bufers that do not contain any data (operation system releases data objects sometimes).
        /// </summary>
        void ClearEmptyBufers();
        void RerenderBufers(BufersFilter bufersFilter = null);
    }
}
