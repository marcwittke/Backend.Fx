using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore
{
    public static class HttpRequestEx
    {
        public static bool IsSafe(this HttpRequest request)
        {
            string method = request.Method.ToUpperInvariant();
            return method == "OPTIONS" || method == "GET" || method == "HEAD";
        }
    }
}