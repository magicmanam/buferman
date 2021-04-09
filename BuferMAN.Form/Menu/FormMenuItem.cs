using BuferMAN.Infrastructure.Menu;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Form.Menu
{
    class FormMenuItem : BufermanMenuItem
    {
        private readonly MenuItem _menuItem;

        private readonly IList<EventHandler> _onClickHandlers = new List<EventHandler>();

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

        public override IEnumerable<BufermanMenuItem> Children
        {
            get
            {
                var result = new List<BufermanMenuItem>();

                foreach (var menuItem in this._menuItem.MenuItems)
                {
                    result.Add(new FormMenuItem(menuItem as MenuItem));
                }

                return result;
            }
        }

        public override void AddOnClickHandler(EventHandler onClick)
        {
            this._onClickHandlers.Add(onClick);
            this._menuItem.Click += onClick;
        }

        public override IEnumerable<EventHandler> GetOnClickHandlers()
        {
            return this._onClickHandlers;
        }

        public override void RemoveOnClickHandler(EventHandler onClick)
        {
            if (this._onClickHandlers.Remove(onClick))
            {
                this._menuItem.Click -= onClick;
            }
        }

        public override void SetOnPopupHandler(EventHandler popup)
        {
            this._menuItem.Popup += popup;
        }

        public override void RemoveOnPopupHandler(EventHandler popup)
        {
            this._menuItem.Popup -= popup;
        }

        public override BufermanMenuItem AddSeparator()
        {
            return new FormMenuItem(this._menuItem.MenuItems.Add("-"));
        }

        public override void AddMenuItem(BufermanMenuItem menuItem)
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