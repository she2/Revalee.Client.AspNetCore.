﻿using System;
using System.Collections;

namespace Revalee.NetCore.Internal
{
    /// <summary>
    /// Represents errors that occur during the processing of a Revalee service request.
    /// </summary>
    public sealed class RevaleeRequestException : Exception
    {
        public RevaleeRequestException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Revalee.Client.RevaleeRequestException"/> class.
        /// </summary>
        /// <param name="serviceBaseUri">A <see cref="T:System.Uri"/> representing the scheme, host, and port for the Revalee service (example: http://localhost:46200).</param>
        /// <param name="callbackUri">An absolute <see cref="T:System.Uri"/> that will be requested on the callback.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public RevaleeRequestException(Uri serviceBaseUri, Uri callbackUri, Exception innerException)
            : base("The request to the Revalee service was unsuccessful.", innerException)
        {
            ServiceBaseUri = serviceBaseUri;
            CallbackUri = callbackUri;
        }


        /// <summary>Gets the service base Uri used to make this Revalee service request.</summary>
        /// <returns>The service base Uri used to make this Revalee service request.</returns>
        public Uri ServiceBaseUri { get; private set; }

        /// <summary>Gets the callback Uri used to make this Revalee service request.</summary>
        /// <returns>The callback Uri used to make this Revalee service request.</returns>
        public Uri CallbackUri { get; private set; }

        /// <summary>
        /// Get a collection of Key/Value pairs that provide additional user-defined information about the exception
        /// </summary>
        public override IDictionary Data
        {
            get
            {
                base.Data.Add("ServiceBaseUri", ServiceBaseUri.ToString());
                base.Data.Add("CallbackUri", CallbackUri.ToString());
                return base.Data;
            }
        }
    }


}
