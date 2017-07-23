using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Revalee.NetCore.Internal;
using Revalee.NetCore.StringKeys;

namespace Revalee.NetCore.Mvc
{
    /// <summary>Represents an attribute that is used to configure callback requests made during this request.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class RevaleeClientSettingsAttribute : ActionFilterAttribute
    {
        private Uri _ServiceBaseUri;
        private int? _RequestTimeout;

        /// <summary>Gets or sets the service base URL used to make callback requests.</summary>
        /// <returns>The service base URL used to make callback requests.</returns>
        public string ServiceBaseUri
        {
            get
            {
                if (_ServiceBaseUri == null)
                {
                    return null;
                }

                return _ServiceBaseUri.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _ServiceBaseUri = null;
                }
                else
                {
                    _ServiceBaseUri = new ServiceBaseUri(value);
                }
            }
        }

        /// <summary>Gets or sets the timeout of callback requests in milliseconds, a value of 0 indicates a default timeout period, a value of -1 indicates an infinite timeout period.</summary>
        /// <returns>The timeout of callback requests in milliseconds, a value of 0 indicates a default timeout period, a value of -1 indicates an infinite timeout period.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="value" /> cannot be a negative value unless -1 for an infinite period.</exception>
        public int RequestTimeout
        {
            get
            {
                if (_RequestTimeout.HasValue)
                {
                    return _RequestTimeout.Value;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (value == 0)
                {
                    _RequestTimeout = null;
                }
                else
                {
                    _RequestTimeout = value;
                }
            }
        }

        /// <summary>Called by the ASP.NET MVC framework before the action method executes.</summary>
        /// <param name="filterContext">The filter context.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="filterContext" /> parameter is null.</exception>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }
            if (filterContext.HttpContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext.HttpContext));
            }

            if (_RequestTimeout.HasValue)
            {

                filterContext.HttpContext.Items[SettingsContextItemKeys.REQUEST_TIMEOUT_APP_SETTING_KEY] = _RequestTimeout;
            }

            if (_ServiceBaseUri != null)
            {
                filterContext.HttpContext.Items[SettingsContextItemKeys.SERVICE_BASE_URI_APP_SETTING_KEY] = _ServiceBaseUri;
            }
        }
    }
}
