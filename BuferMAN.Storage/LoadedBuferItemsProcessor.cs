using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Models;
using BuferMAN.View;
using magicmanam.UndoRedo;
using System;
using System.Collections.Generic;

namespace BuferMAN.Storage
{
    internal class LoadedBuferItemsProcessor : ILoadedBuferItemsProcessor
    {
        private readonly IBuferItemDataObjectConverter _buferItemDataObjectConverter;
        private readonly IIDataObjectHandler _dataObjectHandler;

        public LoadedBuferItemsProcessor(IBuferItemDataObjectConverter buferItemDataObjectConverter, IIDataObjectHandler dataObjectHandler)
        {
            this._buferItemDataObjectConverter = buferItemDataObjectConverter;
            this._dataObjectHandler = dataObjectHandler;
        }

        public void ProcessBuferItems(IEnumerable<BuferItem> bufers)
        {
            using (var action = UndoableContext<ApplicationStateSnapshot>.Current.StartAction(Resource.BufersLoaded))
            {
                var loaded = false;

                foreach (var bufer in bufers)
                {
                    var dataObject = this._buferItemDataObjectConverter.ToDataObject(bufer);
                    var buferViewModel = new BuferViewModel// TODO (s) It should be in mapper and remove reference to Buferman.View assembly
                    {
                        Clip = dataObject,
                        Alias = bufer.Alias,
                        CreatedAt = DateTime.Now,
                        Pinned = bufer.Pinned
                    };

                    var tempLoaded = this._dataObjectHandler.TryHandleDataObject(buferViewModel);
                    loaded = tempLoaded || loaded;
                }

                if (!loaded)
                {
                    action.Cancel();
                }
            }
        }
    }
}
