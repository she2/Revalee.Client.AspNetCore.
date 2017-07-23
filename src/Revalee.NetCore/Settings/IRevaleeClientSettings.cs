using System;

namespace Revalee.NetCore.Settings
{
    internal interface IRevaleeClientSettings
    {
        string AuthorizationKey { get; }

        /// <summary>Gets or sets the timeout of callback requests in milliseconds, a value of null indicates a default timeout period.</summary>
        /// <returns>The timeout of callback requests in milliseconds, a value of null indicates a default timeout period.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="value" /> cannot be negative.</exception>
        int? RequestTimeout { get; }

        /// <summary>Gets or sets the service base Uri used to make callback requests.</summary>
        /// <returns>The service base Uri used to make callback requests.</returns>
        Uri ServiceBaseUri { get; }
    }
}
