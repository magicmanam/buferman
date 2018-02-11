using System;
using static System.Windows.Forms.Menu;

namespace BuferMAN.Menu
{
    public static class MenuItemCollectionExtensions
    {
        public static void AddSeparator(this MenuItemCollection menuItems)
        {
            if (menuItems == null)
            {
                throw new ArgumentNullException(nameof(menuItems));
            }

            menuItems.Add("-");
        }
    }
}
