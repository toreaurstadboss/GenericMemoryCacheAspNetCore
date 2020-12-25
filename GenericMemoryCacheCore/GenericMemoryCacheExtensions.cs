using Microsoft.AspNetCore.Builder;

namespace SomeAcme.SomeUtilNamespace
{
    public static class GenericMemoryCacheExtensions
    {

        public static IApplicationBuilder UseGenericMemoryCache<TItemData>(this IApplicationBuilder builder, GenericMemoryCacheOptions options) where TItemData: class
        {
            return builder.UseMiddleware<GenericMemoryCacheMiddleware<TItemData>>(options);
        }


    }
}
