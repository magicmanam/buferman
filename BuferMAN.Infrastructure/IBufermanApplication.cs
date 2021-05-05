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
        void SaveSession();
        bool IsLatestSessionSaved();
        void RestoreSession();
    }
}
