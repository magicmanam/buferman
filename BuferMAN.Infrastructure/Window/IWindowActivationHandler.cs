﻿using System;

namespace BuferMAN.Infrastructure.Window
{
    public interface IWindowActivationHandler
    {
        void OnActivated(object sender, EventArgs e);
    }
}