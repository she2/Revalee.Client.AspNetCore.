using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Revalee.NetCore.Recurring;
using Revalee.NetCore.StringKeys;

namespace Revalee.NetCore.Configuration
{
    internal sealed class RevaleeRecurringSettingConfiguration : IRevaleeRecurringSettingConfiguration
    {
        readonly IConfiguration _config;
        readonly IConfiguration _rawConfig;

        public RevaleeRecurringSettingConfiguration(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (_rawConfig == null)
            {
                _rawConfig = config;
            }
            _config = config.GetSection(SettingsKey.RecurringSection);
            ChangeToken = config.GetReloadToken();
        }

        public Uri CallbackBaseUri
        {
            get
            {
                var baseUrl = _config[SettingsKey.CallbackBaseUri];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    goto nullError;
                }
                Uri url;

                Uri.TryCreate(baseUrl, UriKind.Absolute, out url);

                if (url == null)
                {
                    goto nullError;
                }


                if (url.Scheme != UriKeys.UriSchemeHttp && url.Scheme != UriKeys.UriSchemeHttps)
                {
                    throw new ArgumentException("The URL attribute only supports http and https.");
                }

                return url;

                nullError:
                throw new ArgumentNullException($"The {nameof(CallbackBaseUri)} is required for Revalee to be able to schedule callbacks");
            }
        }

        public IChangeToken ChangeToken { get; private set; }

        public IList<RecurringTaskModel> TaskModel
        {
            get
            {
                var taskList = _config.GetSection(SettingsKey.TasksList).Get<List<RecurringTaskModel>>();
                //todo Validate
                return taskList;
            }
        }

        public IRevaleeRecurringSettingConfiguration Reload()
        {
            ChangeToken = null;
            return new RevaleeRecurringSettingConfiguration(_rawConfig);
        }
    }
}
