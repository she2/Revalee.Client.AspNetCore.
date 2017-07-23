using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revalee.NetCore.Startup;

namespace Revalee.SampleSite.NetCore
{
    public class RevaleeEvent : TaskEvent
    {
        public override void Activated()
        {
            base.Activated();
        }

        public override void ActivationFailed(ActivationFailureEventArgs args)
        {
            base.ActivationFailed(args);
        }

        public override void Deactivated(DeactivationEventArgs args)
        {
            base.Deactivated(args);
        }
    }
}
