using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revalee.NetCore.Recurring
{
    /// <summary>
    /// Represents a recurring task.
    /// </summary>
    public interface IRecurringTask
    {
        /// <summary>
        /// Gets the unique identifier of the scheduled task.
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Gets the <see cref="T:Revalee.Client.RecurringTasks.PeriodicityType" /> of the scheduled task.
        /// </summary>
        PeriodicityType Periodicity { get; }

        /// <summary>
        /// Gets the <see cref="T:System.Int32" /> value of the scheduled day (1-30).
        /// </summary>
        int DayOffset { get; }

        /// <summary>
        /// Gets the <see cref="T:System.Int32" /> value of the scheduled hour (0-23).
        /// </summary>
        int HourOffset { get; }

        /// <summary>
        /// Gets the <see cref="T:System.Int32" /> value of the scheduled minute (0-59).
        /// /// </summary>
        int MinuteOffset { get; }

        /// <summary>
        /// Gets the <see cref="T:System.Uri" /> defining the target of the callback.
        /// </summary>
        Uri Url { get; }
    }
}
