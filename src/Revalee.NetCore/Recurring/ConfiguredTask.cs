using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Revalee.NetCore.Recurring
{
    internal sealed class ConfiguredTask : IRecurringTask
    {
        private IClockSource _ClockSource;
        private long _LastOccurrence;

        internal ConfiguredTask(string identifier, IClockSource clockSource, PeriodicityType periodicity, int dayOffset, int hourOffset, int minuteOffset, Uri url)
        {
            Identifier = identifier;
            _ClockSource = clockSource;
            Periodicity = periodicity;
            DayOffset = dayOffset;
            HourOffset = hourOffset;
            MinuteOffset = minuteOffset;
            Url = url;
        }

        internal bool HasOccurred => (Interlocked.Read(ref _LastOccurrence) != 0L);

        internal long GetNextOccurrence()
        {
            DateTimeOffset now = _ClockSource.Now;
            DateTimeOffset next;

            switch (Periodicity)
            {
                case PeriodicityType.WithinHourly:
                    next = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Offset);

                    if (next <= now)
                    {
                        next = next.AddMinutes(MinuteOffset);
                    }

                    break;

                case PeriodicityType.Hourly:
                    next = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, MinuteOffset, 0, now.Offset);

                    if (next <= now)
                    {
                        next = next.AddHours(1.0);
                    }

                    break;

                case PeriodicityType.Daily:
                    next = new DateTimeOffset(now.Year, now.Month, now.Day, HourOffset, MinuteOffset, 0, now.Offset);

                    if (next <= now)
                    {
                        next = next.AddDays(1.0);
                    }

                    break;
                case PeriodicityType.Monthly:
                    next = new DateTimeOffset(now.Year, now.Month, DayOffset, HourOffset, MinuteOffset, 0, now.Offset);

                    if (next <= now)
                    {
                        next = next.AddMonths(1);
                    }

                    break;

                default:
                    goto case PeriodicityType.Hourly;
            }

            return next.ToUniversalTime().Ticks;
        }

        internal bool SetLastOccurrence(long occurrence)
        {
            do
            {
                long lastOccurrence = Interlocked.Read(ref _LastOccurrence);

                if (lastOccurrence >= occurrence)
                {
                    return false;
                }

                if (Interlocked.CompareExchange(ref _LastOccurrence, occurrence, lastOccurrence) == lastOccurrence)
                {
                    return true;
                }
            }
            while (true);

        }

        public string Identifier
        {
            get;
            private set;
        }

        public PeriodicityType Periodicity
        {
            get;
            private set;
        }

        public int HourOffset
        {
            get;
            private set;
        }

        public int MinuteOffset
        {
            get;
            private set;
        }

        public Uri Url
        {
            get;
            private set;
        }

        public int DayOffset
        {
            get;
            private set;
        }
    }
}
