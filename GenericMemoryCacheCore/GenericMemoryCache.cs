using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SomeAcme.SomeUtilNamespace
{
    /// <summary>
    /// Thread safe memory cache for generic use - wraps IMemoryCache
    /// </summary>
    /// <typeparam name="TCacheItemData">Payload to store in the memory cache</typeparam>
     /// multiple paralell importing sessions</remarks>
    public class GenericMemoryCache<TCacheItemData> where TCacheItemData : class
    {
        private readonly string _prefixKey;
        private readonly int _defaultExpirationInSeconds;
        private static readonly object _locker = new object();


        public GenericMemoryCache(IMemoryCache memoryCache, string prefixKey, int defaultExpirationInSeconds = 0)
        {
            defaultExpirationInSeconds = Math.Abs(defaultExpirationInSeconds); //checking if a negative value was passed into the constructor.

            _prefixKey = prefixKey;
            Cache = memoryCache;
            _defaultExpirationInSeconds = defaultExpirationInSeconds;
        }

        /// <summary>
        /// Cache object if direct access is desired. Only allow exposing this for inherited types.
        /// </summary>
        protected IMemoryCache Cache { get; }

        public string PrefixKey(string key) => $"{_prefixKey}_{key}"; //to avoid IMemoryCache collisions with other parts of the same process, each cache key is always prefixed with a set prefix set by the constructor of this class.


        /// <summary>
        /// Adds an item to memory cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="itemToCache"></param>
        /// <returns></returns>
        public bool AddItem(string key, TCacheItemData itemToCache)
        {
            try
            {
                if (!key.StartsWith(_prefixKey))
                    key = PrefixKey(key);

                lock (_locker)
                {
                    if (!Cache.TryGetValue(key, out TCacheItemData existingItem))
                    {
                        var cts = new CancellationTokenSource(_defaultExpirationInSeconds > 0 ?
                            _defaultExpirationInSeconds * 1000 : -1);
                        var cacheEntryOptions = new MemoryCacheEntryOptions().AddExpirationToken(new CancellationChangeToken(cts.Token));

                        Cache.Set(key, itemToCache, cacheEntryOptions);
                        return true;
                    }
                }
                return false; //Item not added, the key already exists
            }
            catch (Exception err)
            {
                Debug.WriteLine(err);
                return false;
            }
        }

        public virtual List<T> GetValues<T>()
        {
            lock (_locker)
            {
                List<T> values = Cache.GetKeys<T>().ToList();
                return values;
            }
        }



        /// <summary>
        /// Retrieves a cache item. Possible to set the expiration of the cache item in seconds. 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TCacheItemData GetItem(string key)
        {
            try
            {
                if (!key.StartsWith(_prefixKey))
                    key = PrefixKey(key);
                lock (_locker)
                {
                    if (Cache.TryGetValue(key, out TCacheItemData cachedItem))
                    {
                        return cachedItem;
                    }
                }
                return null;

            }
            catch (Exception err)
            {
                Debug.WriteLine(err);
                return null;
            }
        }

        public bool SetItem(string key, TCacheItemData itemToCache)
        {
            try
            {
                if (!key.StartsWith(_prefixKey))
                    key = PrefixKey(key);
                lock (_locker)
                {
                    if (GetItem(key) != null)
                    {
                        AddItem(key, itemToCache);
                        return true;
                    }
                    UpdateItem(key, itemToCache);
                }
                return true;
            }
            catch (Exception err)
            {
                Debug.WriteLine(err);
                return false;
            }
        }


        /// <summary>
        /// Updates an item in the cache and set the expiration of the cache item 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="itemToCache"></param>
        /// <returns></returns>
        public bool UpdateItem(string key, TCacheItemData itemToCache)
        {
            if (!key.StartsWith(_prefixKey))
                key = PrefixKey(key);
            lock (_locker)
            {
                TCacheItemData existingItem = GetItem(key);
                if (existingItem != null)
                {
                    //always remove the item existing before updating
                    RemoveItem(key);
                }
                AddItem(key, itemToCache);
            }
            return true;

        }

        /// <summary>
        /// Removes an item from the cache 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool RemoveItem(string key)
        {
            if (!key.StartsWith(_prefixKey))
                key = PrefixKey(key);

            lock (_locker)
            {
                if (Cache.TryGetValue(key, out var item))
                {
                    if (item != null)
                    {

                    }
                    Cache.Remove(key);
                    return true;
                }
            }
            return false;
        }

        public void AddItems(Dictionary<string, TCacheItemData> itemsToCache)
        {
            foreach (var kvp in itemsToCache)
                AddItem(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Clear all cache keys starting with known prefix passed into the constructor.
        /// </summary>
        public void ClearAll()
        {
            lock (_locker)
            {
                List<string> cacheKeys = Cache.GetKeys<string>().Where(k => k.StartsWith(_prefixKey)).ToList();

                foreach (string cacheKey in cacheKeys)
                {
                    if (cacheKey.StartsWith(_prefixKey))
                        Cache.Remove(cacheKey);
                }
            }
        }
        

    }
}


