using System;
using Revalee.NetCore.Settings;
using Revalee.NetCore.StringKeys;

namespace Revalee.NetCore.Internal
{
    /// <summary>Creates a <see cref="T:System.Uri"/> for use as the base Uri for the Revalee service.</summary>
    public sealed class ServiceBaseUri : Uri
    {
        /// <summary>Initializes a new instance of the <see cref="T:Revalee.Client.ServiceBaseUri" /> class with the configured identifier.</summary>
        /// <exception cref="T:System.UriFormatException">The configured value is not valid as a service base Uri for the Revalee service.</exception>
        internal ServiceBaseUri(IRevaleeClientSettings config)
            : base(BuildConfiguredServiceBase(config), UriKind.Absolute)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:Revalee.Client.ServiceBaseUri" /> class with the specified identifier.</summary>
        /// <param name="serviceHost">A DNS-style domain name, IP address, or full URL for the Revalee service.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="serviceHost" /> is null.</exception>
        /// <exception cref="T:System.UriFormatException"><paramref name="serviceHost" /> is not valid as a service base Uri for the Revalee service.</exception>
        public ServiceBaseUri(string serviceHost)
            : base(BuildSpecifiedServiceBase(serviceHost), UriKind.Absolute)
        {
        }


        private ServiceBaseUri(string uri, UriKind kind)
            : base(uri, kind)
        {
        }

        /// <summary>Creates a new instance of the <see cref="T:Revalee.Client.ServiceBaseUri" /> class with the specified identifier.</summary>
        /// <returns>A <see cref="T:System.Boolean" /> value that is true if the <see cref="T:Revalee.Client.ServiceBaseUri" /> was successfully created; otherwise, false.</returns>
        /// <param name="serviceHost">A DNS-style domain name, IP address, or full URL for the Revalee service.</param>
        /// <param name="uri">When this method returns, contains the constructed <see cref="T:Revalee.Client.ServiceBaseUri" />.</param>
        public static bool TryCreate(string serviceHost, out ServiceBaseUri uri)
        {
            if (!string.IsNullOrWhiteSpace(serviceHost))
            {
                if (serviceHost.IndexOfAny(new char[] { ':', '/' }, 0) < 0)
                {
                    if (Uri.CheckHostName(serviceHost) != UriHostNameType.Unknown)
                    {
                        uri = new ServiceBaseUri(new UriBuilder(ServiceBaseUriKeys.DEFAULT_SERVICE_SCHEME, serviceHost, ServiceBaseUriKeys.DEFAULT_HTTP_PORT_NUMBER).ToString(), UriKind.Absolute);
                        return true;
                    }
                }
                else
                {
                    Uri proxyUri = null;

                    if (serviceHost.IndexOf(UriKeys.SchemeDelimiter, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        Uri.TryCreate(string.Concat(ServiceBaseUriKeys.DEFAULT_SERVICE_SCHEME, UriKeys.SchemeDelimiter, serviceHost), UriKind.Absolute, out proxyUri);
                    }
                    else
                    {
                        Uri.TryCreate(serviceHost, UriKind.Absolute, out proxyUri);
                    }

                    if (proxyUri != null
                        && proxyUri.HostNameType != UriHostNameType.Unknown
                        && proxyUri.IsAbsoluteUri
                        && (UriKeys.UriSchemeHttp.Equals(proxyUri.Scheme) || UriKeys.UriSchemeHttps.Equals(proxyUri.Scheme)))
                    {
                        if (proxyUri.IsDefaultPort)
                        {
                            if (UriKeys.UriSchemeHttp.Equals(proxyUri.Scheme, StringComparison.OrdinalIgnoreCase)
                                && serviceHost.LastIndexOf(":80", StringComparison.Ordinal) < (serviceHost.Length - 3))
                            {
                                // Incorrect default port
                                uri = new ServiceBaseUri(new UriBuilder(proxyUri.Scheme, proxyUri.Host, ServiceBaseUriKeys.DEFAULT_HTTP_PORT_NUMBER).ToString(), UriKind.Absolute);
                                return true;
                            }
                            else if (UriKeys.UriSchemeHttps.Equals(proxyUri.Scheme, StringComparison.OrdinalIgnoreCase)
                                && serviceHost.LastIndexOf(":443", StringComparison.Ordinal) < (serviceHost.Length - 4))
                            {
                                // Incorrect default port
                                uri = new ServiceBaseUri(new UriBuilder(proxyUri.Scheme, proxyUri.Host, ServiceBaseUriKeys.DEFAULT_HTTPS_PORT_NUMBER).ToString(), UriKind.Absolute);
                                return true;
                            }
                        }
                        else
                        {
                            uri = new ServiceBaseUri(proxyUri.ToString(), UriKind.Absolute);
                            return true;
                        }
                    }
                }
            }

            uri = null;
            return false;
        }

        private static string BuildConfiguredServiceBase(IRevaleeClientSettings config)
        {
            Uri configuredServiceBaseUri = config.ServiceBaseUri;

            if (configuredServiceBaseUri == null)
            {
                return new UriBuilder(ServiceBaseUriKeys.DEFAULT_SERVICE_SCHEME, ServiceBaseUriKeys.DFAULT_SERVICE_HOST, ServiceBaseUriKeys.DEFAULT_HTTP_PORT_NUMBER).ToString();
            }

            return configuredServiceBaseUri.ToString();
        }

        private static string BuildSpecifiedServiceBase(string serviceHost)
        {
            if (string.IsNullOrWhiteSpace(serviceHost))
            {
                throw new ArgumentNullException(nameof(serviceHost));
            }

            if (serviceHost.IndexOfAny(new char[] { ':', '/' }, 0) < 0)
            {
                if (Uri.CheckHostName(serviceHost) == UriHostNameType.Unknown)
                {
                    throw new ArgumentException("Invalid host name specified for service host.", nameof(serviceHost));
                }

                return (new UriBuilder(ServiceBaseUriKeys.DEFAULT_SERVICE_SCHEME, serviceHost, ServiceBaseUriKeys.DEFAULT_HTTP_PORT_NUMBER)).ToString();
            }
            else
            {
                try
                {
                    Uri proxyUri;

                    if (serviceHost.IndexOf(UriKeys.SchemeDelimiter, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        proxyUri = new Uri(string.Concat(ServiceBaseUriKeys.DEFAULT_SERVICE_SCHEME, UriKeys.SchemeDelimiter, serviceHost), UriKind.Absolute);
                    }
                    else
                    {
                        proxyUri = new Uri(serviceHost, UriKind.Absolute);
                    }

                    if (proxyUri.HostNameType == UriHostNameType.Unknown)
                    {
                        throw new ArgumentException("Invalid host name specified for service host.", nameof(serviceHost));
                    }

                    if (!proxyUri.IsAbsoluteUri || !(UriKeys.UriSchemeHttp.Equals(proxyUri.Scheme) || UriKeys.UriSchemeHttps.Equals(proxyUri.Scheme)))
                    {
                        throw new ArgumentException("Invalid scheme specified for service host.", nameof(serviceHost));
                    }

                    if (proxyUri.IsDefaultPort)
                    {
                        if (UriKeys.UriSchemeHttp.Equals(proxyUri.Scheme, StringComparison.OrdinalIgnoreCase)
                            && serviceHost.LastIndexOf(":80", StringComparison.Ordinal) < (serviceHost.Length - 3))
                        {
                            // Incorrect default port
                            return (new UriBuilder(proxyUri.Scheme, proxyUri.Host, ServiceBaseUriKeys.DEFAULT_HTTP_PORT_NUMBER)).ToString();
                        }
                        else if (UriKeys.UriSchemeHttps.Equals(proxyUri.Scheme, StringComparison.OrdinalIgnoreCase)
                            && serviceHost.LastIndexOf(":443", StringComparison.Ordinal) < (serviceHost.Length - 4))
                        {
                            // Incorrect default port
                            return (new UriBuilder(proxyUri.Scheme, proxyUri.Host, ServiceBaseUriKeys.DEFAULT_HTTPS_PORT_NUMBER)).ToString();
                        }
                    }

                    return proxyUri.ToString();
                }
                catch (UriFormatException ufex)
                {
                    throw new ArgumentException("Invalid format specified for service host.", nameof(serviceHost), ufex);
                }
            }
        }
    }
}
