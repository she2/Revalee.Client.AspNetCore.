using System;
using System.Collections.Generic;
using Revalee.NetCore.Recurring;

namespace Revalee.NetCore.Settings
{
    internal interface IRevaleeClientRecurringSettings
    {
        IList<RecurringTaskModel> TaskModel { get; }
        Uri CallbackBaseUri { get; }
    }
}
