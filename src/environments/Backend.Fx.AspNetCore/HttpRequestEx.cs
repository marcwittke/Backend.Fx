using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore
{
    [PublicAPI]
    public static class HttpRequestEx
    {
        /// <summary>
        /// Is the request method considered as safe in sense of a RESTful API?
        /// See https://restcookbook.com/HTTP%20Methods/idempotency/
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsRestfulSafe(this HttpRequest request)
        {
            return request.IsGet() || request.IsOptions() || request.IsHead();
        }
        
        /// <summary>
        /// Is the request method considered as idempotent in sense of a RESTful API?
        /// See https://restcookbook.com/HTTP%20Methods/idempotency/
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsRestfulIdempotent(this HttpRequest request)
        {
            return request.IsGet() || request.IsOptions() || request.IsHead() || request.IsDelete() || request.IsPut();
        }

        public static bool IsGet(this HttpRequest request)
        {
            return HttpMethods.IsGet(request.Method);
        }
        
        public static bool IsConnect(this HttpRequest request)
        {
            return HttpMethods.IsConnect(request.Method);
        }
        
        public static bool IsDelete(this HttpRequest request)
        {
            return HttpMethods.IsDelete(request.Method);
        }
        
        public static bool IsHead(this HttpRequest request)
        {
            return HttpMethods.IsHead(request.Method);
        }
        
        public static bool IsOptions(this HttpRequest request)
        {
            return HttpMethods.IsOptions(request.Method);
        }
        
        public static bool IsPatch(this HttpRequest request)
        {
            return HttpMethods.IsPatch(request.Method);
        }
        
        public static bool IsPost(this HttpRequest request)
        {
            return HttpMethods.IsPost(request.Method);
        }
        
        public static bool IsPut(this HttpRequest request)
        {
            return HttpMethods.IsPut(request.Method);
        }
        
        public static bool IsTrace(this HttpRequest request)
        {
            return HttpMethods.IsTrace(request.Method);
        }
    }
}