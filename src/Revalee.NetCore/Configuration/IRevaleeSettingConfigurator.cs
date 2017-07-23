using Microsoft.Extensions.Configuration;

namespace Revalee.NetCore.Configuration
{
    internal interface IRevaleeSettingConfigurator
    {
        IRevaleeClientSettingConfiguration ClientSettingConfiguration { get; }
        IRevaleeSettingConfigurator AddClientConfig(IConfiguration config);
        IRevaleeRecurringSettingConfiguration RecurringSettingConfiguration { get; }
        IRevaleeSettingConfigurator AddRecurringConfig(IConfiguration config);
    }
}
