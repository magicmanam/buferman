using BuferMAN.Infrastructure.Menu;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Form.Menu
{
    class FormMenuItem : BuferMANMenuItem
    {
        private readonly MenuItem _menuItem;

        public FormMenuItem(string text, EventHandler eventHandler = null)
        {
            this._menuItem = new MenuItem(text, eventHandler);
        }

        public FormMenuItem(MenuItem menuItem)
        {
            this._menuItem = menuItem;
        }

        public override string Text
        {
            get
            {
                return this._menuItem.Text;
            }
            set
            {
                this._menuItem.Text = value;
            }
        }

        public override Shortcut ShortCut
        {
            get => this._menuItem.Shortcut;
            set
            {
                this._menuItem.Shortcut = value;
            }
        }

        public override bool Enabled { get => this._menuItem.Enabled; set => this._menuItem.Enabled = value; }

        public override bool Checked { get => this._menuItem.Checked; set => this._menuItem.Checked = value; }

        public override IEnumerable<BuferMANMenuItem> Children
        {
            get
            {
                var result = new List<BuferMANMenuItem>();

                foreach (var menuItem in this._menuItem.MenuItems)
                {
                    result.Add(new FormMenuItem(menuItem as MenuItem));
                }

                return result;
            }
        }

        public override void SetOnClickHandler(EventHandler click)
        {
            this._menuItem.Click += click;
        }

        public override void RemoveOnClickHandler(EventHandler click)
        {
            this._menuItem.Click -= click;
        }

        public override void SetOnPopupHandler(EventHandler popup)
        {
            this._menuItem.Popup += popup;
        }

        public override void RemoveOnPopupHandler(EventHandler popup)
        {
            this._menuItem.Popup -= popup;
        }

        public override BuferMANMenuItem AddSeparator()
        {
            return new FormMenuItem(this._menuItem.MenuItems.Add("-"));
        }

        public override void AddMenuItem(BuferMANMenuItem menuItem)
        {
            this._menuItem.MenuItems.Add((menuItem as FormMenuItem).GetMenuItem());
        }

        internal MenuItem GetMenuItem()
        {
            return this._menuItem;
        }

        public override void Remove()
        {
            this._menuItem.Parent.MenuItems.Remove(this._menuItem);
        }
    }
}
