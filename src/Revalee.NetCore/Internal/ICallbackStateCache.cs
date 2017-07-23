using System;

namespace Revalee.NetCore.Internal
{
    internal interface ICallbackStateCache
    {
        void StoreCallbackState(Guid callbackId, object state, DateTime expirationTime);

        void StoreCallbackState(Guid callbackId, object state, TimeSpan expirationTimeSpan);

        void DeleteCallbackState(Guid callbackId);
        object RecoverCallbackState(Guid callbackId);

        object RecoverCallbackState(string cacheKey);
    }
}
