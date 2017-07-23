using System;
using Microsoft.Extensions.Primitives;

namespace Revalee.NetCore.Configuration
{
    internal interface IRevaleeClientSettingConfiguration
    {
        Uri ServiceBaseUri { get; }
        int? RequestTimeout { get; }
        string AuthorizationKey { get; }
        IChangeToken ChangeToken { get; }
        IRevaleeClientSettingConfiguration Reload();
    }
}
