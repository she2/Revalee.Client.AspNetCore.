using System;

namespace Revalee.NetCore.Recurring
{
    public sealed class RecurringTaskModel
    {
        public PeriodicityType Periodicity { get; set; }
        public int? Day { get; set; }
        public int? Hour { get; set; }
        public int Minute { get; set; }
        public Uri Url { get; set; }
    }
}
