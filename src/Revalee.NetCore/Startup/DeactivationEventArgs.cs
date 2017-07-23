using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revalee.NetCore.Internal;

namespace Revalee.NetCore.Startup
{
    public sealed class DeactivationEventArgs
    {
        /// <summary>
        /// Creates an instance of the <see cref="T:Revalee.Client.RecurringTasks.DeactivationEventArgs"/> class.
        /// </summary>
        /// <param name="exception">A <see cref="T:Revalee.Client.RevaleeRequestException"/> that is the cause of the deactivation.</param>
        public DeactivationEventArgs(RevaleeRequestException exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Gets the <see cref="T:Revalee.Client.RevaleeRequestException"/> that caused the deactivation.
        /// </summary>
        public RevaleeRequestException Exception { get; }
    }
}
