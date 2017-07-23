using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Revalee.NetCore.Internal
{
    internal sealed class CallbackStateCache : ICallbackStateCache
    {
        readonly IMemoryCache _cache;

        public CallbackStateCache(IMemoryCache cache)
        {
            _cache = cache;
        }
        public void StoreCallbackState(Guid callbackId, object state, DateTime expirationTime)
        {
            if (Guid.Empty.Equals(callbackId))
            {
                throw new ArgumentNullException(nameof(callbackId));
            }

            //if (context.Cache == null)
            //{
            //    return;
            //}

            if (state == null)
            {
                RemoveFromCache(callbackId.ToString());
                return;
            }
            AddToCache(callbackId.ToString(), state, expirationTime);
        }

        public void StoreCallbackState(Guid callbackId, object state, TimeSpan expirationTimeSpan)
        {
            if (Guid.Empty.Equals(callbackId))
            {
                throw new ArgumentNullException(nameof(callbackId));
            }

            //if (context.Cache == null)
            //{
            //    return;
            //}

            if (state == null)
            {
                RemoveFromCache(callbackId.ToString());
                return;
            }

            AddToCache(callbackId.ToString(), state, DateTime.Now.Add(expirationTimeSpan));
        }

        public void DeleteCallbackState(Guid callbackId)
        {
            RemoveFromCache(callbackId.ToString());
        }

        public object RecoverCallbackState(Guid callbackId)
        {
            return RemoveFromCache(callbackId.ToString());
        }

        public object RecoverCallbackState(string cacheKey)
        {
            return RemoveFromCache(cacheKey);
        }

        private void AddToCache(string cacheKey, object state, DateTime expirationTime)
        {
            if (_cache == null)
            {
                throw new ArgumentNullException(nameof(_cache));
            }
            if (cacheKey == null)
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }
            var options = new MemoryCacheEntryOptions();
            options.SetPriority(CacheItemPriority.High);
            options.SetAbsoluteExpiration(expirationTime);

            _cache.Set(cacheKey, state, options);
        }

        private object RemoveFromCache(string cacheKey)
        {
            if (_cache == null)
            {
                throw new ArgumentNullException(nameof(_cache));
            }
            if (cacheKey == null)
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }
            var cachedValue = _cache.Get(cacheKey);
            _cache.Remove(cacheKey);
            return cachedValue;
        }
    }
}
