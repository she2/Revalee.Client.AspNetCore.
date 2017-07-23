using System;
using Microsoft.Extensions.Configuration;
using Revalee.NetCore.StringKeys;

namespace Revalee.NetCore.Configuration
{
    internal sealed class RevaleeSettingConfigurator : IRevaleeSettingConfigurator
    {
        IRevaleeClientSettingConfiguration _clientConfig;
        IRevaleeRecurringSettingConfiguration _recurringConfig;
        private void OnReloadClientConfig(object state)
        {
            _clientConfig = ClientSettingConfiguration.Reload();

            if (ClientSettingConfiguration.ChangeToken != null)
            {
                ClientSettingConfiguration.ChangeToken.RegisterChangeCallback(OnReloadClientConfig, null);
            }

        }
        private void OnReloadRecurringConfig(object state)
        {
            _recurringConfig = RecurringSettingConfiguration.Reload();

            if (RecurringSettingConfiguration.ChangeToken != null)
            {
                RecurringSettingConfiguration.ChangeToken.RegisterChangeCallback(OnReloadRecurringConfig, null);
            }
        }

        public IRevaleeSettingConfigurator AddClientConfig(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            _clientConfig = new RevaleeClientSettingConfiguration(config.GetSection(SettingsKey.RevaleeSection));
            if (ClientSettingConfiguration.ChangeToken != null)
            {
                ClientSettingConfiguration.ChangeToken.RegisterChangeCallback(OnReloadClientConfig, null);
            }

            return this;
        }
        public IRevaleeSettingConfigurator AddRecurringConfig(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            _recurringConfig = new RevaleeRecurringSettingConfiguration(config.GetSection(SettingsKey.RevaleeSection));
            if (RecurringSettingConfiguration.ChangeToken != null)
            {
                RecurringSettingConfiguration.ChangeToken.RegisterChangeCallback(OnReloadRecurringConfig, null);
            }

            return this;
        }

        public IRevaleeClientSettingConfiguration ClientSettingConfiguration => _clientConfig;
        public IRevaleeRecurringSettingConfiguration RecurringSettingConfiguration => _recurringConfig;


    }
}
