using System.Collections.Generic;
using System.Windows.Forms;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;

namespace BuferMAN.WinForms
{
    public class Bufer : IBufer
    {
        private Button _button;// TODO (m) add readonly modifier

        public Bufer()
        {
            this._button = new Button();
        }

        public void SetButton(Button button)
        {
            this._button = button;
        }

        public void SetContextMenu(IEnumerable<BufermanMenuItem> menuItems)
        {
            this._button.ContextMenu = new ContextMenu();
            this._button.ContextMenu.PopulateMenuWithItems(menuItems);
        }
    }
}
