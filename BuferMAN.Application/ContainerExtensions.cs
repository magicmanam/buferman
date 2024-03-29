﻿using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Window;
using SimpleInjector;

namespace BuferMAN.Application
{
    public static class ContainerExtensions
    {
        public static Container RegisterApplicationPart(this Container container)
        {
            container.Register<IBufermanApplication, BufermanApplication>(Lifestyle.Singleton);
            container.Register<IIDataObjectHandler, DataObjectHandler>(Lifestyle.Singleton);
            container.Register<ITime, XTime>(Lifestyle.Singleton);
            container.Register<IRenderingHandler, RenderingHandler>(Lifestyle.Singleton);

            return container;
        }
    }
}
