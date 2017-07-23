using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Revalee.NetCore.Recurring;

namespace Revalee.NetCore.Configuration
{
    internal interface IRevaleeRecurringSettingConfiguration
    {
        IList<RecurringTaskModel> TaskModel { get; }
        Uri CallbackBaseUri { get; }
        IChangeToken ChangeToken { get; }
        IRevaleeRecurringSettingConfiguration Reload();
    }
}
