using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Revalee.NetCore.Configuration;
using Revalee.NetCore.Recurring;

namespace Revalee.NetCore.Settings
{
    /// <summary>Represents the configured values used to make callback requests.</summary>
    internal sealed class RevaleeClientRecurringSettings : IRevaleeClientRecurringSettings
    {
        private const string _authorizationKeyAppSettingsKey = "RevaleeAuthorizationKey";
        private const string _requestTimeoutAppSettingsKey = "RevaleeRequestTimeout";
        private const string _serviceBaseUriAppSettingsKey = "RevaleeCallbackBaseUri";
        readonly HttpContext _context;
        IRevaleeSettingConfigurator _clientSetting;

        public RevaleeClientRecurringSettings(IHttpContextAccessor context, IRevaleeSettingConfigurator clientSetting)
        {
            _context = context?.HttpContext;
            _clientSetting = clientSetting;
        }

        public IList<RecurringTaskModel> TaskModel
        {
            get
            {
                if (_clientSetting != null)
                {
                    var configuredTask = _clientSetting.RecurringSettingConfiguration.TaskModel;

                    if (configuredTask != null)
                    {
                        foreach (var taskUrl in configuredTask)
                        {

                        }
                        return configuredTask;
                    }
                }

                return null;
            }
        }

        public Uri CallbackBaseUri
        {
            get
            {

                if (_clientSetting != null)
                {
                    Uri configuredValue = _clientSetting.RecurringSettingConfiguration.CallbackBaseUri;

                    if (configuredValue != null)
                    {
                        return configuredValue;
                    }
                }

                return null;
            }
        }
    }
}
