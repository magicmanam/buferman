using System.Collections.Generic;
using System.Windows.Forms;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;

namespace BuferMAN.Form
{
    public class Bufer : IBufer
    {
        private Button _button;// TODO add readonly modifier

        public Bufer()
        {
            this._button = new Button();
        }

        public void SetButton(Button button)
        {
            this._button = button;
        }

        public void SetContextMenu(IEnumerable<BuferMANMenuItem> menuItems)
        {
            this._button.ContextMenu = new System.Windows.Forms.ContextMenu();
            this._button.ContextMenu.PopulateMenuWithItems(menuItems);
        }
    }
}
