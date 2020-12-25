using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using SomeAcme.SomeUtilNamespace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
            context.Request.EnableBuffering(); //do this to be able to re-read the body multiple times without consuming it. (asp.net core 3.1)

            if (context.Request.Method.ToLower() == "post") {

                if (IsDefinedCacheOperation("addtocache", context))
                {

                    // Leave the body open so the next middleware can read it.
                    using (var reader = new StreamReader(
                        context.Request.Body,
                        encoding: Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: false,
                        bufferSize: 4096,
                        leaveOpen: true))
                    {
                        var body = await reader.ReadToEndAsync();
                        // Do some processing with body
                        if (body != null)
                        {
                            string cacheKey = context.Request.Query["cachekey"].ToString();

                            if (context.Request.Query.ContainsKey("type"))
                            {

                                var typeArgs = CreateGenericCache(context, memoryCache, out var cache);

                                var payloadItem = JsonConvert.DeserializeObject(body, typeArgs[0]);
                                var addMethod = cache.GetType().GetMethod("AddItem");
                                if (addMethod != null)
                                {
                                    addMethod.Invoke(cache, new[] {cacheKey, payloadItem});
                                }

                            }
                            else
                            {

                                var cache = new GenericMemoryCache<object>(memoryCache, cacheKey, 0);
                                if (cache != null)
                                {
                                    //TODO: implement
                                }
                            }


                        }
                    }

                    // Reset the request body stream position so the next middleware can read it
                    context.Request.Body.Position = 0;

                }

            }

            if (context.Request.Method.ToLower() == "delete")
            {
                if (IsDefinedCacheOperation("removeitemfromcache", context))
                {
                    var typeArgs = CreateGenericCache(context, memoryCache, out var cache);
                    var removeMethod = cache.GetType().GetMethod("RemoveItem");

                    string cacheKey = context.Request.Query["cachekey"].ToString();

                    if (removeMethod != null)
                    {
                        removeMethod.Invoke(cache, new[] { cacheKey });
                    }

                }

            }

            if (context.Request.Method.ToLower() == "get")
            {
                if (IsDefinedCacheOperation("getvaluesfromcache", context))
                {

                    var typeArgs = CreateGenericCache(context, memoryCache, out var cache);

                    var getValuesMethod = cache.GetType().GetMethod("GetValues");
                    if (getValuesMethod != null)
                    {
                        var genericGetValuesMethod = getValuesMethod.MakeGenericMethod(typeArgs);
                        var existingValuesInCache = genericGetValuesMethod.Invoke(cache, null);
                        if (existingValuesInCache != null)
                        {

                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(existingValuesInCache));
                        }
                        else
                        {
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync("{}"); //return empty object literal
                        }

                        return; //terminate further processing - return data 
                    }

                }

            }

            await _next(context);

        }

        private static bool IsDefinedCacheOperation(string cacheOperation, HttpContext context, bool requireType = true)
        {
            return context.Request.Query.ContainsKey(cacheOperation) &&
                   context.Request.Query.ContainsKey("prefix") && (!requireType || context.Request.Query.ContainsKey("type"));
        }

        private static Type[] CreateGenericCache(HttpContext context, IMemoryCache memoryCache, out object cache)
        {
            Type genericType = typeof(GenericMemoryCache<>);
            string cacheitemtype = context.Request.Query["type"].ToString();
            string prefix = context.Request.Query["prefix"].ToString();
            Type[] typeArgs = {Type.GetType(cacheitemtype)};
            Type cacheType = genericType.MakeGenericType(typeArgs);
            cache = Activator.CreateInstance(cacheType, memoryCache, prefix, 0);
            return typeArgs;
        }
    }
}
