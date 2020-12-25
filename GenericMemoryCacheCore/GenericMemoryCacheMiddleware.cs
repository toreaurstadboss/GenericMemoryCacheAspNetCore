using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using SomeAcme.SomeUtilNamespace;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SomeAcme.SomeUtilNamespace
{
    public class GenericMemoryCacheMiddleware<TCacheItemData> where TCacheItemData: class
    {

        private readonly RequestDelegate _next;
        private readonly string _prefixKey;
        private readonly int _defaultExpirationTimeInSeconds;

        public GenericMemoryCacheMiddleware(RequestDelegate next, GenericMemoryCacheOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _next = next;
            _prefixKey = options.PrefixKey;
            _defaultExpirationTimeInSeconds = options.DefaultExpirationInSeconds; 
        }

        public async Task InvokeAsync(HttpContext context, IMemoryCache memoryCache)
        {
            new GenericMemoryCache<TCacheItemData>(memoryCache, _prefixKey, _defaultExpirationTimeInSeconds);
            await _next(context);

        }


    }
}
