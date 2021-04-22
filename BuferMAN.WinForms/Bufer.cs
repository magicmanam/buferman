using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.View;

namespace BuferMAN.WinForms
{
    internal class Bufer : IBufer
    {
        private readonly Button _button;
        private readonly ToolTip _focusTooltip;
        private readonly ToolTip _mouseOverTooltip;
        private readonly IList<EventHandler> _onClickHandlers = new List<EventHandler>();
        private readonly IList<EventHandler> _onFocusHandlers = new List<EventHandler>();
        private readonly IList<EventHandler> _onUnfocusHandlers = new List<EventHandler>();

        public Bufer()
        {
            this._button = new Button()
            {
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0)
            };
            this._focusTooltip = new ToolTip()
            {
                OwnerDraw = false
            };
            this._mouseOverTooltip = new ToolTip()
            {
                InitialDelay = 0,
                IsBalloon = true
            };
        }

        public Button GetButton()
        {
            return this._button;
        }

        public void SetContextMenu(IEnumerable<BufermanMenuItem> menuItems)
        {
            this._button.ContextMenu = new ContextMenu();
            this._button.ContextMenu.PopulateMenuWithItems(menuItems);
        }

        public string Text
        {
            get
            {
                return this._button.Text;
            }
            set
            {
                this._button.Text = value;
            }
        }

        public BuferViewModel ViewModel { get; set; }

        public Font ApplyFontStyle(FontStyle style)
        {
            this._button.Font = new Font(this._button.Font, style);

            return this._button.Font;
        }

        public void AddOnClickHandler(EventHandler onClick)
        {
            this._onClickHandlers.Add(onClick);
            this._button.Click += onClick;
        }

        public IEnumerable<EventHandler> GetOnClickHandlers()
        {
            return this._onClickHandlers;
        }

        public void RemoveOnClickHandler(EventHandler onClick)
        {
            if (this._onClickHandlers.Remove(onClick))
            {
                this._button.Click -= onClick;
            }
        }

        public void AddOnFocusHandler(EventHandler onFocus)
        {
            this._onFocusHandlers.Add(onFocus);
            this._button.GotFocus += onFocus;
        }

        public IEnumerable<EventHandler> GetOnFocusHandlers()
        {
            return this._onFocusHandlers;
        }

        public void RemoveOnFocusHandler(EventHandler onFocus)
        {
            if (this._onFocusHandlers.Remove(onFocus))
            {
                this._button.GotFocus -= onFocus;
            }
        }

        public void AddOnUnfocusHandler(EventHandler onUnfocus)
        {
            this._onUnfocusHandlers.Add(onUnfocus);
            this._button.LostFocus += onUnfocus;
        }

        public IEnumerable<EventHandler> GetOnUnfocusHandlers()
        {
            return this._onUnfocusHandlers;
        }

        public void RemoveOnUnfocusHandler(EventHandler onUnfocus)
        {
            if (this._onUnfocusHandlers.Remove(onUnfocus))
            {
                this._button.LostFocus -= onUnfocus;
            }
        }

        public Color BackColor
        {
            get
            {
                return this._button.BackColor;
            }
            set
            {
                this._button.BackColor = value;
            }
        }

        public ToolTip FocusTooltip
        {
            get
            {
                return this._focusTooltip;
            }
        }

        public ToolTip MouseOverTooltip
        {
            get
            {
                return this._mouseOverTooltip;
            }
        }

        public int Width
        {
            get
            {
                return this._button.Width;
            }
            set
            {
                this._button.Width = value;
            }
        }

        public void SetMouseOverToolTip(string tooltipText)
        {
            this.MouseOverTooltip.SetToolTip(this._button, tooltipText);
        }

        public void ShowFocusTooltip(string tooltipText, int durationInMilliseconds)
        {
            this.FocusTooltip.Show(tooltipText, this._button, durationInMilliseconds);
        }

        public void HideFocusTooltip()
        {
            this.FocusTooltip.Hide(this._button);
        }

        public int TabIndex
        {
            get
            {
                return this._button.TabIndex;
            }
        }
    }
}
