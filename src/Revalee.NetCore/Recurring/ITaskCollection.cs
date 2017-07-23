using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revalee.NetCore.Recurring
{
    internal interface ITaskCollection
    {
        int Count { get; }

        IEnumerable<ConfiguredTask> Tasks { get; }

        bool TryGetTask(string identifier, out ConfiguredTask taskConfig);

        Task<bool> AddAsync(ConfiguredTask taskConfig);

        Task<bool> RemoveAsync(string identifier);

        Task<bool> RemoveAsync(ConfiguredTask taskConfig);
    }
}
