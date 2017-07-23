using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revalee.NetCore.Startup
{
    /// <summary>
    /// Represents event arguments for a failure to activate the recurring task module.
    /// </summary>
    public sealed class ActivationFailureEventArgs
    {
        /// <summary>
        /// Creates an instance of the <see cref="T:Revalee.Client.RecurringTasks.ActivationFailureEventArgs"/> class.
        /// </summary>
        /// <param name="failureCount">A <see cref="T:System.Int32"/> value representing the number of consecutive failures to activate.</param>
        public ActivationFailureEventArgs(int failureCount)
        {
            if (failureCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(failureCount));
            }

            FailureCount = failureCount;
        }

        /// <summary>
        /// Gets the number of consecutive failures.
        /// </summary>
        public int FailureCount { get; }
    }
}
