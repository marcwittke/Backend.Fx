using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore
{
    public static class HttpResponseEx
    {
        public static async Task WriteJsonAsync(this HttpResponse response, object o, string contentType = null)
        {
            await response.WriteJsonAsync(System.Text.Json.JsonSerializer.Serialize(o), contentType);
        }
        
        public static async Task WriteJsonAsync(this HttpResponse response, string json, string contentType = null)
        {
            response.ContentType = (contentType ?? "application/json; charset=UTF-8");
            await response.WriteAsync(json);
            await response.Body.FlushAsync();
        }
    }
}