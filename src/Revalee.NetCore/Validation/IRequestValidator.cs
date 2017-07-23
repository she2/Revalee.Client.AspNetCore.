using System;

namespace Revalee.NetCore.Validation
{
    public interface IRequestValidator
    {
        string Issue(Uri callbackUri);
        bool Validate(string authorizationHeaderValue, Guid callbackId, Uri callbackUri);
    }
}
