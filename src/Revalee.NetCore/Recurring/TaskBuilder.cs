using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Revalee.NetCore.Recurring
{
    internal sealed class TaskBuilder : IDisposable
    {
        private Uri _callbackBaseUri;
        private HashAlgorithm _HashAlgorithm;

        internal TaskBuilder()
            : this(null)
        {
        }

        internal TaskBuilder(Uri callbackBaseUri)
        {
            _callbackBaseUri = callbackBaseUri;
            _HashAlgorithm = SHA256.Create();
        }

        internal ConfiguredTask Create(IClockSource clockSource, PeriodicityType periodicity, int dayOffset, int hourOffset, int minuteOffset, Uri url)
        {
            Uri absoluteUrl;

            if (url.IsAbsoluteUri)
            {
                absoluteUrl = url;
            }
            else
            {
                if (_callbackBaseUri == null)
                {
                    throw new InvalidOperationException(string.Format("The recurring task targeting \"{0}\" is not an absolute URL and no callbackBaseUri attribute was supplied.", url));
                }

                if (!Uri.TryCreate(_callbackBaseUri, url, out absoluteUrl))
                {
                    throw new InvalidOperationException(string.Format("The recurring task targeting \"{0}\" is not an absolute URL and it cannot be combined with the callbackBaseUri attribute of \"{1}\".", url, _callbackBaseUri));
                }
            }

            string identifier = CreateTaskIdentifier(periodicity, hourOffset, dayOffset, minuteOffset, absoluteUrl);
            return new ConfiguredTask(identifier, clockSource, periodicity, dayOffset, hourOffset, minuteOffset, absoluteUrl);
        }

        private string CreateTaskIdentifier(PeriodicityType periodicity, int dayOffSet, int hourOffset, int minuteOffset, Uri url)
        {
            string keyFormat;

            switch (periodicity)
            {
                case PeriodicityType.Hourly:
                    keyFormat = "H~XX:{2:00}:{3:00}~{4}";
                    break;

                case PeriodicityType.Daily:
                    keyFormat = "D~{1:00}:{2:00}:{3:00}~{4}";
                    break;

                //case PeriodicityType.Monthly:
                //    keyFormat = "{0}~{1:00}:{2:00}{3:00}~{4}";
                //    break;

                default:
                    keyFormat = "{0}~{1:00}:{2:00}:{3:00}~{4}";
                    break;
            }

            string compoundKey = string.Format(CultureInfo.InvariantCulture, keyFormat, (int)periodicity, dayOffSet, hourOffset, minuteOffset, url);
            byte[] textBytes = Encoding.UTF8.GetBytes(compoundKey);
            byte[] hashBytes = _HashAlgorithm.ComputeHash(textBytes);
            return ConvertByteArrayToHexadecimalString(hashBytes);
        }

        private static string ConvertByteArrayToHexadecimalString(byte[] bytes)
        {
            char[] charArray = new char[bytes.Length * 2];
            int byteValue;

            for (int index = 0; index < bytes.Length; index++)
            {
                byteValue = bytes[index] >> 4;
                charArray[index * 2] = (char)(55 + byteValue + (((byteValue - 10) >> 31) & -7));
                byteValue = bytes[index] & 0xF;
                charArray[index * 2 + 1] = (char)(55 + byteValue + (((byteValue - 10) >> 31) & -7));
            }

            return new string(charArray);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_HashAlgorithm != null)
                {
                    _HashAlgorithm.Dispose();
                    _HashAlgorithm = null;
                }

                GC.SuppressFinalize(this);
            }
        }

        ~TaskBuilder()
        {
            this.Dispose(false);
        }
    }
}
