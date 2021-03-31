using BuferMAN.Form.Menu;
using BuferMAN.Infrastructure.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuferMAN.Form
{
    static class MenuExtensions
    {
        public static void PopulateMenuWithItems(this System.Windows.Forms.Menu menu, IEnumerable<BuferMANMenuItem> menuItems)
        {
            foreach (var menuItem in menuItems)
            {
                menu.MenuItems.Add((menuItem as FormMenuItem).GetMenuItem());
            }
        }
    }
}
