using BuferMAN.Infrastructure.Menu;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.WinForms.Menu
{
    class FormMenuItem : BufermanMenuItem
    {
        private readonly MenuItem _menuItem;
        private Func<string> _textFn;
        private IList<BufermanMenuItem> _children = new List<BufermanMenuItem>();

        private readonly IList<EventHandler> _onClickHandlers = new List<EventHandler>();

        public FormMenuItem(Func<string> textFn, EventHandler eventHandler = null) : this(textFn(), eventHandler)
        {
            this._textFn = textFn;
        }

        public FormMenuItem(string text, EventHandler eventHandler = null)
        {
            this._menuItem = new MenuItem(text, eventHandler);
        }

        public FormMenuItem(MenuItem menuItem)
        {
            this._menuItem = menuItem;
        }

        public override void TextRefresh()
        {
            if (this._textFn != null)
            {
                this.Text = this._textFn();

                foreach (var child in this.Children)
                {
                    child.TextRefresh();
                }
            }
        }

        public override void SetTextFunction(Func<string> textFunc)
        {
            if (textFunc != null)
            {
                this._textFn = textFunc;
            }
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
                return this._children;
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
            this._children.Add(menuItem);
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