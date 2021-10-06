using System;
using BuferMAN.Infrastructure;

namespace BuferMAN.Application
{
    class XTime : ITime
    {
        public DateTime LocalTime => DateTime.Now;
    }
}
