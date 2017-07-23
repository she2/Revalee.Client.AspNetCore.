using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Revalee.NetCore.Settings;
using Revalee.NetCore.StringKeys;
using Revalee.NetCore.Validation;

namespace Revalee.NetCore.Internal
{
    internal sealed class SchedulingAgent : IRevaleeRegistrar
    {
        readonly IRequestValidator _validator;
        readonly IRevaleeClientSettings _clientSetting;
        private const int _DefaultRequestTimeoutInMilliseconds = 13000;
        private const string _RevaleeAuthHttpHeaderName = "Revalee-Auth";
        HttpContext _context;
        readonly Lazy<HttpClient> _LazyHttpClient;

        public SchedulingAgent(IRequestValidator validator, IRevaleeClientSettings clientSetting, IHttpContextAccessor context)
        {
            _validator = validator;
            _clientSetting = clientSetting;
            _context = context?.HttpContext;

            _LazyHttpClient = new Lazy<HttpClient>(() => InitializeHttpClient(GetWebRequestTimeout()), true);
        }

        public Task<Guid> RequestCallbackAsync(Uri callbackUri, DateTimeOffset callbackTime) => RequestCallbackAsync(callbackUri, callbackTime, CancellationToken.None);

        public async Task<Guid> RequestCallbackAsync(Uri callbackUri, DateTimeOffset callbackTime, CancellationToken cancellationToken)
        {
            if (callbackUri == null)
            {
                throw new ArgumentNullException(nameof(callbackUri));
            }

            if (!callbackUri.IsAbsoluteUri)
            {
                throw new ArgumentException("Callback Uri is not an absolute Uri.", nameof(callbackUri));
            }

            var serviceBaseUri = new ServiceBaseUri(_clientSetting);

            try
            {
                bool isDisposalRequired;
                HttpClient httpClient = AcquireHttpClient(out isDisposalRequired);

                try
                {
                    Uri requestUri = BuildScheduleRequestUri(serviceBaseUri, callbackTime.UtcDateTime, callbackUri);
                    string authorizationHeaderValue = _validator.Issue(callbackUri);
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Put, requestUri))
                    {
                        if (!string.IsNullOrEmpty(authorizationHeaderValue))
                        {
                            requestMessage.Headers.Add(_RevaleeAuthHttpHeaderName, authorizationHeaderValue);
                        }

                        using (HttpResponseMessage response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                return Guid.ParseExact(responseText, "D");
                            }
                            else
                            {
                                throw new RevaleeRequestException(serviceBaseUri, callbackUri,
                                    new WebException(string.Format("The remote server returned an error: ({0}) {1}.",
                                        (int)response.StatusCode, response.ReasonPhrase), WebExceptionStatus.ProtocolError));
                            }
                        }
                    }
                }
                finally
                {
                    if (isDisposalRequired)
                    {
                        httpClient.Dispose();
                    }
                }
            }
            catch (AggregateException aex)
            {
                throw new RevaleeRequestException(serviceBaseUri, callbackUri, aex.Flatten().InnerException);
            }
            catch (WebException wex)
            {
                throw new RevaleeRequestException(serviceBaseUri, callbackUri, wex);
            }
        }

        public Task<bool> CancelCallbackAsync(Guid callbackId, Uri callbackUri) => CancelCallbackAsync(callbackId, callbackUri, CancellationToken.None);

        public async Task<bool> CancelCallbackAsync(Guid callbackId, Uri callbackUri, CancellationToken cancellationToken)
        {
            if (Guid.Empty.Equals(callbackId))
            {
                throw new ArgumentNullException(nameof(callbackUri));
            }

            if (callbackUri == null)
            {
                throw new ArgumentNullException(nameof(callbackUri));
            }

            if (!callbackUri.IsAbsoluteUri)
            {
                throw new ArgumentException("Callback Uri is not an absolute Uri.", nameof(callbackUri));
            }

            var serviceBaseUri = new ServiceBaseUri(_clientSetting);

            try
            {
                bool isDisposalRequired;
                HttpClient httpClient = AcquireHttpClient(out isDisposalRequired);

                try
                {
                    Uri requestUri = BuildCancelRequestUri(serviceBaseUri, callbackId, callbackUri);
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Put, requestUri))
                    {
                        using (HttpResponseMessage response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                return true;
                            }
                            else
                            {
                                throw new RevaleeRequestException(serviceBaseUri, callbackUri,
                                    new WebException(string.Format("The remote server returned an error: ({0}) {1}.",
                                        (int)response.StatusCode, response.ReasonPhrase), WebExceptionStatus.ProtocolError));
                            }
                        }
                    }
                }
                finally
                {
                    if (isDisposalRequired)
                    {
                        httpClient.Dispose();
                    }
                }
            }
            catch (AggregateException aex)
            {
                throw new RevaleeRequestException(serviceBaseUri, callbackUri, aex.Flatten().InnerException);
            }
            catch (WebException wex)
            {
                throw new RevaleeRequestException(serviceBaseUri, callbackUri, wex);
            }
        }


        public bool ValidateCallback(HttpContext suppliedContext = null)
        {
            var contextToUse = _context ?? suppliedContext;
            if (contextToUse == null)
            {
                throw new ArgumentNullException("request");
            }

            if (contextToUse.Request?.Host.Host == null)
            {
                return false;
            }
            if (contextToUse.Request == null || contextToUse.Request.Form == null)
            {
                return false;
            }
            string authorizationHeader = contextToUse.Request.Headers[_RevaleeAuthHttpHeaderName];

            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return false;
            }

            Guid callbackId;
            if (!Guid.TryParse(contextToUse.Request.Form["callbackId"], out callbackId))
            {
                return false;
            }
            var callbackUrl = $"{contextToUse.Request.Scheme}{UriKeys.SchemeDelimiter}{contextToUse.Request.Host.ToUriComponent()}{contextToUse.Request.Path.ToUriComponent()}";
            return _validator.Validate(authorizationHeader, callbackId, new Uri(callbackUrl));
        }


        private Uri BuildScheduleRequestUri(Uri serviceBaseUri, DateTime callbackUtcTime, Uri callbackUri)
            => new Uri(string.Format(CultureInfo.InvariantCulture, "{0}://{1}/Schedule?CallbackTime={2:s}Z&CallbackUrl={3}", serviceBaseUri.Scheme, serviceBaseUri.Authority, callbackUtcTime, PrepareCallbackUrl(callbackUri)), UriKind.Absolute);

        private Uri BuildCancelRequestUri(Uri serviceBaseUri, Guid callbackId, Uri callbackUri)
            => new Uri(string.Format(CultureInfo.InvariantCulture, "{0}://{1}/Cancel?CallbackId={2:D}Z&CallbackUrl={3}", serviceBaseUri.Scheme, serviceBaseUri.Authority, callbackId, PrepareCallbackUrl(callbackUri)), UriKind.Absolute);

        private string PrepareCallbackUrl(Uri callbackUri) => Uri.EscapeDataString(callbackUri.OriginalString);

        private HttpClient AcquireHttpClient(out bool isDisposalRequired)
        {
            if (_LazyHttpClient.IsValueCreated)
            {
                TimeSpan currentTimeoutSetting = GetWebRequestTimeout();
                HttpClient httpClient = _LazyHttpClient.Value;

                if (httpClient.Timeout == currentTimeoutSetting)
                {
                    isDisposalRequired = false;
                    return httpClient;
                }
                else
                {
                    isDisposalRequired = true;
                    return InitializeHttpClient(currentTimeoutSetting);
                }
            }
            else
            {
                isDisposalRequired = false;
                return _LazyHttpClient.Value;
            }
        }

        private HttpClient InitializeHttpClient(TimeSpan timeout)
        {
            HttpClient httpClient;
            var httpHandler = new HttpClientHandler();

            try
            {
                httpHandler.AllowAutoRedirect = false;
                //httpHandler.MaxRequestContentBufferSize = 1024;
                httpHandler.UseCookies = false;
                httpHandler.UseDefaultCredentials = false;

                httpClient = new HttpClient(httpHandler, true);
            }
            catch
            {
                httpHandler.Dispose();
                throw;
            }

            httpClient.DefaultRequestHeaders.ExpectContinue = false;
            httpClient.DefaultRequestHeaders.UserAgent.Clear();
            httpClient.DefaultRequestHeaders.UserAgent.Add(GetUserAgent());
            httpClient.Timeout = timeout;
            httpClient.MaxResponseContentBufferSize = 1024;
            return httpClient;
        }

        private ProductInfoHeaderValue GetUserAgent()
        {
            Assembly callingAssembly = Assembly.GetEntryAssembly();
            AssemblyName assemblyName = callingAssembly.GetName();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(callingAssembly.Location);
            return new ProductInfoHeaderValue(assemblyName.Name, versionInfo.ProductVersion);
        }

        private TimeSpan GetWebRequestTimeout()
        {
            int? configuredRequestTimeout = _clientSetting.RequestTimeout;
            if (configuredRequestTimeout.HasValue)
            {
                return TimeSpan.FromMilliseconds(configuredRequestTimeout.Value);
            }
            else
            {
                return TimeSpan.FromMilliseconds(_DefaultRequestTimeoutInMilliseconds);
            }
        }
    }
}
