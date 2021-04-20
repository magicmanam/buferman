using BuferMAN.WinForms.Menu;
using BuferMAN.Infrastructure.Menu;
using System.Collections.Generic;

namespace BuferMAN.WinForms
{
    static class MenuExtensions
    {
        public static void PopulateMenuWithItems(this System.Windows.Forms.Menu menu, IEnumerable<BufermanMenuItem> menuItems)
        {
            foreach (var menuItem in menuItems)
            {
                menu.MenuItems.Add((menuItem as FormMenuItem).GetMenuItem());
            }
        }
    }
}
