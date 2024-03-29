﻿using System;
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
        private IEnumerable<BufermanMenuItem> _contextMenuItems;

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

        public IEnumerable<BufermanMenuItem> ContextMenu
        {
            get
            {
                return this._contextMenuItems;
            }
            set
            {
                this._contextMenuItems = value;
                this._button.ContextMenu = new System.Windows.Forms.ContextMenu();// TODO (m) remove reference to BuferMAN.ContextMenu (2 places in this project)
                this._button.ContextMenu.PopulateMenuWithItems(value);
            }
        }

        public void SetText(string text)
        {
            this._button.Text = text;
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
            this._mouseOverTooltip.SetToolTip(this._button, tooltipText);
        }

        public void SetMouseOverToolTipTitle(string tooltipTitle)
        {
            this._mouseOverTooltip.ToolTipTitle = tooltipTitle;
        }

        public void ShowFocusTooltip(string tooltipText, int durationInMilliseconds)
        {
            this._focusTooltip.Show(tooltipText, this._button, durationInMilliseconds);
        }

        public void HideFocusTooltip()
        {
            this._focusTooltip.Hide(this._button);
        }

        public int TabIndex
        {
            get
            {
                return this._button.TabIndex;
            }
            set
            {
                this._button.TabIndex = value;
            }
        }

        public Point Location
        {
            get
            {
                return this._button.Location;
            }
            set
            {
                this._button.Location = value;
            }
        }

        public Font Font
        {
            get
            {
                return this._button.Font;
            }
        }

        public Image Image
        {
            get
            {
                return this._button.Image;
            }
            set
            {
                this._button.Image = value;
            }
        }
        public ContentAlignment ImageAlign
        {
            get
            {
                return this._button.ImageAlign;
            }
            set
            {
                this._button.ImageAlign = value;
            }
        }
        public ContentAlignment TextAlign { get
            {
                return this._button.TextAlign;
            }
            set
            {
                this._button.TextAlign = value;
            }
        }
    }
}
