using System;
using System.Security.Cryptography;
using System.Text;
using Revalee.NetCore.Settings;
using Revalee.NetCore.StringKeys;

namespace Revalee.NetCore.Validation
{
    /// <summary>
    /// Helper methods to cryptographically ensure that callbacks are only processed when legitimately requested by this application.
    /// </summary>
    internal sealed class RequestValidator : IRequestValidator
    {
        private const string _DefaultClientKey = "Revalee.Authorization";
        private const short _CurrentVersion = 2;
        readonly IRevaleeClientSettings _revaleeClientSettings;

        public RequestValidator(IRevaleeClientSettings revaleeClientSettings)
        {
            _revaleeClientSettings = revaleeClientSettings;
        }
        /// <summary>
        /// Creates a cipher to be used to validate legitimate callbacks.
        /// </summary>
        /// <param name="callbackUri">An absolute <see cref="T:System.Uri"/> that will be requested on the callback.</param>
        /// <returns>A cipher value for this callback.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="callbackUri" /> is null.</exception>
        public string Issue(Uri callbackUri)
        {
            if (callbackUri == null || string.IsNullOrEmpty(callbackUri.OriginalString))
            {
                throw new ArgumentNullException(nameof(callbackUri));
            }

            short version = CurrentVersion;
            byte[] nonce = GenerateNonce();
            byte[] clientKey = RetrieveClientKey();
            byte[] subject = GetSubject(callbackUri);
            byte[] clientCryptogram = BuildClientCryptogram(nonce, subject, clientKey);

            return new AuthorizationCipher(AuthorizationCipher.CipherSource.Client, version, nonce, clientCryptogram).ToString();
        }

        /// <summary>
        /// Validates the cipher to ensure it represents a legitimately requested callback.
        /// </summary>
        /// <param name="authorizationHeaderValue">A cipher value for this callback.</param>
        /// <param name="callbackId">A <see cref="T:System.Guid"/> that serves as an identifier for the scheduled callback.</param>
        /// <param name="callbackUri">An absolute <see cref="T:System.Uri"/> that will be requested on the callback.</param>
        /// <returns>true if the cipher is valid, false if not.</returns>
        public bool Validate(string authorizationHeaderValue, Guid callbackId, Uri callbackUri)
        {
            if (string.IsNullOrEmpty(authorizationHeaderValue))
            {
                return false;
            }

            if (Guid.Empty.Equals(callbackId))
            {
                return false;
            }

            if (callbackUri == null || string.IsNullOrEmpty(callbackUri.OriginalString))
            {
                return false;
            }

            AuthorizationCipher incomingCipher;
            if (!AuthorizationCipher.TryParse(authorizationHeaderValue, out incomingCipher) || incomingCipher.Source != AuthorizationCipher.CipherSource.Server)
            {
                return false;
            }

            short version = incomingCipher.Version;

            switch (version)
            {
                case _CurrentVersion:

                    byte[] nonce = incomingCipher.Nonce;
                    byte[] clientKey = RetrieveClientKey();
                    byte[] subject = GetSubject(callbackUri);
                    byte[] clientCryptogram = BuildClientCryptogram(nonce, subject, clientKey);
                    byte[] responseId = GetResponseId(callbackId);
                    byte[] serverCryptogram = BuildServerCryptogram(nonce, clientCryptogram, responseId);

                    return AreEqual(serverCryptogram, incomingCipher.Cryptogram);

                default:

                    return false;
            }
        }

        private short CurrentVersion
        {
            get
            {
                return _CurrentVersion;
            }
        }

        private byte[] GenerateNonce()
        {
            byte[] nonceBytes = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(nonceBytes);
            }

            return nonceBytes;
        }

        private byte[] RetrieveClientKey()
        {
            string key = _revaleeClientSettings.AuthorizationKey;

            if (string.IsNullOrEmpty(key))
            {
                //key = HostingEnvironment.SiteName;

                //if (string.IsNullOrEmpty(key))
                //{
                // Default will be a hard-coded string
                key = _DefaultClientKey;
                //}
            }

            return Encoding.UTF8.GetBytes(key);
        }

        private byte[] GetSubject(Uri callbackUri)
        {
            if (callbackUri.IsDefaultPort)
            {
                // Remove the port number from the callbackUri if it is the default port for the specified protocol
                return Encoding.UTF8.GetBytes(RemoveDefaultPortFromOriginalString(callbackUri));
            }
            else
            {
                return Encoding.UTF8.GetBytes(callbackUri.OriginalString);
            }
        }

        private string RemoveDefaultPortFromOriginalString(Uri callbackUri)
        {
            string canonicalizedUriString = callbackUri.OriginalString;

            if (callbackUri.Scheme == UriKeys.UriSchemeHttp)
            {
                int portDelimiterIndex = UriKeys.UriSchemeHttp.Length + UriKeys.SchemeDelimiter.Length + callbackUri.Authority.Length;

                if (canonicalizedUriString[portDelimiterIndex] == ':')
                {
                    canonicalizedUriString = canonicalizedUriString.Substring(0, portDelimiterIndex) + canonicalizedUriString.Substring(portDelimiterIndex + ":80".Length);
                }
            }
            else if (callbackUri.Scheme == UriKeys.UriSchemeHttps)
            {
                int portDelimiterIndex = UriKeys.UriSchemeHttps.Length + UriKeys.SchemeDelimiter.Length + callbackUri.Authority.Length;

                if (canonicalizedUriString[portDelimiterIndex] == ':')
                {
                    canonicalizedUriString = canonicalizedUriString.Substring(0, portDelimiterIndex) + canonicalizedUriString.Substring(portDelimiterIndex + ":443".Length);
                }
            }

            return canonicalizedUriString;
        }

        private byte[] GetResponseId(Guid callbackId)
        {
            return callbackId.ToByteArray();
        }

        private byte[] BuildClientCryptogram(byte[] nonce, byte[] subject, byte[] clientKey)
        {
            byte[] contents = new byte[nonce.Length + subject.Length];
            Array.Copy(nonce, 0, contents, 0, nonce.Length);
            Array.Copy(subject, 0, contents, nonce.Length, subject.Length);

            using (var hmac = new HMACSHA256(clientKey))
            {
                return hmac.ComputeHash(contents);
            }
        }

        private byte[] BuildServerCryptogram(byte[] nonce, byte[] clientCryptogram, byte[] responseId)
        {
            byte[] contents = new byte[nonce.Length + clientCryptogram.Length + responseId.Length];

            Array.Copy(nonce, 0, contents, 0, nonce.Length);
            Array.Copy(clientCryptogram, 0, contents, nonce.Length, clientCryptogram.Length);
            Array.Copy(responseId, 0, contents, nonce.Length + clientCryptogram.Length, responseId.Length);

            using (var hashingAlgorithm = SHA256.Create())
            {
                return hashingAlgorithm.ComputeHash(contents);
            }
        }

        private static bool AreEqual(byte[] array1, byte[] array2)
        {
            if (array1 == array2)
            {
                return true;
            }

            if (array1 == null || array2 == null || array1.Length != array2.Length)
            {
                return false;
            }

            for (int byteIndex = 0; byteIndex < array1.Length; byteIndex++)
            {
                if (array1[byteIndex] != array2[byteIndex])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
