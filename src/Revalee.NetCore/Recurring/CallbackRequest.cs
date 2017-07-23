using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revalee.NetCore.Recurring
{
    internal sealed class CallbackRequest
    {
        internal CallbackRequest(DateTimeOffset callbackTime, Uri callbackUri)
        {
            this.CallbackTime = callbackTime;
            this.CallbackUri = callbackUri;
        }

        internal DateTimeOffset CallbackTime
        {
            get;
            private set;
        }

        internal Uri CallbackUri
        {
            get;
            private set;
        }
    }
}
