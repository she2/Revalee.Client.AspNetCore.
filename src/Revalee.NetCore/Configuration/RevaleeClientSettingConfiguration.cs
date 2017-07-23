using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Revalee.NetCore.StringKeys;

namespace Revalee.NetCore.Configuration
{
    internal sealed class RevaleeClientSettingConfiguration : IRevaleeClientSettingConfiguration
    {
        readonly IConfiguration _config;
        readonly IConfiguration _rawConfig;

        public RevaleeClientSettingConfiguration(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (_rawConfig == null)
            {
                _rawConfig = config;
            }
            _config = config.GetSection(SettingsKey.ClientSection);
            ChangeToken = config.GetReloadToken();
        }

        public string AuthorizationKey => _config[SettingsKey.AuthorizationKey];

        public IChangeToken ChangeToken { get; private set; }

        public int? RequestTimeout
        {
            get
            {
                var timeOut = _config[SettingsKey.RequestTimeout];
                if (string.IsNullOrEmpty(timeOut))
                {
                    return null;
                }

                int revaleeTimeOut;
                int.TryParse(timeOut, out revaleeTimeOut);

                return revaleeTimeOut;

            }

        }

        public Uri ServiceBaseUri
        {
            get
            {
                var baseUrl = _config[SettingsKey.ServiceBaseUri];
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
                throw new ArgumentNullException($"The {nameof(ServiceBaseUri)} is required for Revalee to be able to schedule callbacks");
            }
        }

        public IRevaleeClientSettingConfiguration Reload()
        {
            ChangeToken = null;
            return new RevaleeClientSettingConfiguration(_rawConfig);
        }
    }
}
