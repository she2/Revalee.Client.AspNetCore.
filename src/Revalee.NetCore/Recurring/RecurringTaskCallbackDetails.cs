using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revalee.NetCore.Recurring
{
    public sealed class RecurringTaskCallbackDetails
    {
        private string _callbackId;
        private string _callbackTime;
        private string _currentServiceTime;
        private string _taskIdentifier;

        internal RecurringTaskCallbackDetails(string callbackId, string callbackTime, string currentServiceTime, string taskIdentifier)
        {
            _callbackId = callbackId;
            _callbackTime = callbackTime;
            _currentServiceTime = currentServiceTime;
            _taskIdentifier = taskIdentifier;
        }

        /// <summary>
        /// Gets the <see cref="T:System.String" /> identifying the task that launched this callback.
        /// </summary>
        public Guid TaskIdentifier
        {
            get
            {
                return Guid.Parse(_taskIdentifier);
            }
        }

        /// <summary>
        /// Gets the <see cref="T:System.Guid" /> identifying this callback.
        /// </summary>
        public Guid CallbackId
        {
            get
            {
                return Guid.Parse(_callbackId);
            }
        }

        /// <summary>
        /// Gets the scheduled <see cref="T:System.DateTimeOffset" /> of this callback.
        /// </summary>
        public DateTimeOffset CallbackTime
        {
            get
            {
                return DateTimeOffset.Parse(_callbackTime).ToLocalTime();
            }
        }

        /// <summary>
        /// Gets the <see cref="T:System.DateTimeOffset" /> of the moment this callback was issued according to the Revalee service that issued the callback.
        /// </summary>
        public DateTimeOffset CurrentServiceTime
        {
            get
            {
                return DateTimeOffset.Parse(_currentServiceTime).ToLocalTime();
            }
        }
    }
}
