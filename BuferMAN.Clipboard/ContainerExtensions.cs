using SimpleInjector;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Clipboard
{
    public static class ContainerExtensions
    {
        public static Container RegisterClipboardPart(this Container container)
        {
            container.Register<IClipboardWrapper, ClipboardWrapper>(Lifestyle.Singleton);
            container.Register<IEqualityComparer<IDataObject>>(() => new DataObjectComparer(ClipboardFormats.StringFormats, ClipboardFormats.FileFormats), Lifestyle.Singleton);
            container.Register<IClipboardBuferService, ClipboardBuferService>(Lifestyle.Singleton);

            return container;
        }
    }
}
