using System;
using Microsoft.AspNetCore.Http;
using Revalee.NetCore.Configuration;
using Revalee.NetCore.StringKeys;

namespace Revalee.NetCore.Settings
{
    /// <summary>Represents the configured values used to make callback requests.</summary>
    internal sealed class RevaleeClientSettings : IRevaleeClientSettings
    {

        readonly HttpContext _context;
        readonly IRevaleeSettingConfigurator _clientSetting;

        public RevaleeClientSettings(IHttpContextAccessor context, IRevaleeSettingConfigurator clientSetting)
        {
            _context = context?.HttpContext;
            _clientSetting = clientSetting;
        }

        public string AuthorizationKey
        {
            get
            {

                if (_context != null)
                {
                    string overrideValue = _context.Items[SettingsContextItemKeys.AUTHORIZATION_KEY_APP_SETTING_KEY] as string;

                    if (!string.IsNullOrEmpty(overrideValue))
                    {
                        return overrideValue;
                    }
                }

                if (_clientSetting != null)
                {
                    string configuredValue = _clientSetting.ClientSettingConfiguration.AuthorizationKey;

                    if (!string.IsNullOrEmpty(configuredValue))
                    {
                        return configuredValue;
                    }
                }

                return null;
            }
        }

        /// <summary>Gets or sets the timeout of callback requests in milliseconds, a value of null indicates a default timeout period.</summary>
        /// <returns>The timeout of callback requests in milliseconds, a value of null indicates a default timeout period.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="value" /> cannot be negative.</exception>
        public int? RequestTimeout
        {
            get
            {

                if (_context != null)
                {
                    object overrideValue = _context.Items[SettingsContextItemKeys.REQUEST_TIMEOUT_APP_SETTING_KEY];

                    if (overrideValue != null && overrideValue is int)
                    {
                        int overrideInt = (int)overrideValue;

                        if (overrideInt > 0 || overrideInt == -1)
                        {
                            return overrideInt;
                        }
                    }
                }

                if (_clientSetting != null)
                {
                    int? configuredValue = _clientSetting.ClientSettingConfiguration.RequestTimeout;

                    if (configuredValue != null && (configuredValue > 0 || configuredValue == -1))
                    {
                        return configuredValue;
                    }
                }
                return null;
            }
            set
            {
                if (value.HasValue && value.Value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }


                if (_context != null)
                {
                    if (value.HasValue && (value.Value > 0 || value.Value == -1))
                    {
                        _context.Items[SettingsContextItemKeys.REQUEST_TIMEOUT_APP_SETTING_KEY] = value;
                    }
                    else
                    {
                        _context.Items.Remove(SettingsContextItemKeys.REQUEST_TIMEOUT_APP_SETTING_KEY);
                    }
                }
            }
        }

        /// <summary>Gets or sets the service base Uri used to make callback requests.</summary>
        /// <returns>The service base Uri used to make callback requests.</returns>
        public Uri ServiceBaseUri
        {
            get
            {

                if (_context != null)
                {
                    object overrideValue = _context.Items[SettingsContextItemKeys.SERVICE_BASE_URI_APP_SETTING_KEY];

                    Uri overrideUri = overrideValue as Uri;

                    if (overrideUri != null)
                    {
                        if (overrideUri.IsAbsoluteUri)
                        {
                            return overrideUri;
                        }
                    }
                }

                if (_clientSetting != null)
                {
                    Uri configuredValue = _clientSetting.ClientSettingConfiguration.ServiceBaseUri;

                    if (configuredValue != null)
                    {
                        return configuredValue;
                    }
                }

                return null;
            }
            set
            {
                if (_context != null)
                {
                    if (value != null && value.IsAbsoluteUri)
                    {
                        _context.Items[SettingsContextItemKeys.SERVICE_BASE_URI_APP_SETTING_KEY] = value;
                    }
                    else
                    {
                        _context.Items.Remove(SettingsContextItemKeys.SERVICE_BASE_URI_APP_SETTING_KEY);
                    }
                }
            }
        }
    }
}
