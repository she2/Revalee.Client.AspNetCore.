using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revalee.NetCore.Recurring
{
    public interface IRecurringTaskModule
    {
        ITaskManifest GetManifest();
        Task Restart();
        RecurringTaskCallbackDetails CallbackDetails { get; }
        bool IsProcessingRecurringCallback { get; }
    }
}
