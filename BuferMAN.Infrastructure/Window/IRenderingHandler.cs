using System;

namespace BuferMAN.Infrastructure.Window
{
    public interface IRenderingHandler
    {
        void Render(IBufermanHost bufermanHost);
    }
}