using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revalee.NetCore.Startup
{
    public class RevaleeOptions
    {
        public TaskEvent TaskEvent { get; set; } = new TaskEvent();
    }
}
