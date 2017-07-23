using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revalee.NetCore.Recurring
{
    /// <summary>
    /// Represents a time source used to schedule recurring tasks.
    /// </summary>
    public interface IClockSource
    {
        /// <summary>
        /// Gets the current <see cref="T:System.DateTimeOffset" /> to be used for scheduling callbacks.
        /// </summary>
        DateTimeOffset Now { get; }
    }
}
