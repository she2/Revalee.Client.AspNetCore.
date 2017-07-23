using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revalee.NetCore.Recurring
{
    internal sealed class RequestAnalysis
    {
        internal bool IsRecurringTask = false;
        internal string TaskIdentifier = string.Empty;
        internal long Occurrence = 0;
    }
}
