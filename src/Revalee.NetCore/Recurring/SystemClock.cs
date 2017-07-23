using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revalee.NetCore.Recurring
{
    internal sealed class SystemClock : IClockSource
    {
        public static readonly IClockSource Instance = new SystemClock();

        private SystemClock()
        {
        }

        public DateTimeOffset Now
        {
            get
            {
                return DateTimeOffset.Now;
            }
        }
    }
}
