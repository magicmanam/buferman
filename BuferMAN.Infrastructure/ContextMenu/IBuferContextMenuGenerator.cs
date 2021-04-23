﻿using BuferMAN.Infrastructure.Menu;
using System.Collections.Generic;

namespace BuferMAN.Infrastructure.ContextMenu
{
    public interface IBuferContextMenuGenerator
    {
        IEnumerable<BufermanMenuItem> GenerateContextMenuItems(IBufer bufer,
            bool isChangeTextAvailable,
            IBuferSelectionHandler buferSelectionHandler,
            IBufermanHost bufermanHost,
            IBuferTypeMenuGenerator buferTypeGenerator);
    }
}
