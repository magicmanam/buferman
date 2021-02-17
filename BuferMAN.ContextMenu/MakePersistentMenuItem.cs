﻿using BuferMAN.ContextMenu.Properties;
using Microsoft.VisualBasic;
using System;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class MakePersistentMenuItem : MenuItem
    {
        public MakePersistentMenuItem()
        {
            this.Text = Resource.MenuPersistent;
            this.Shortcut = Shortcut.CtrlS;
        }
    }
}
