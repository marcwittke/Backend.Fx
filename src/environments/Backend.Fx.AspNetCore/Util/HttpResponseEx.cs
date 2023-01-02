using System.Text.Json;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore.Util;

[PublicAPI]
public static class HttpResponseEx
{
    public static async Task WriteJsonAsync(this HttpResponse response, object o, JsonSerializerOptions options = null, string contentType = null)
    {
        options ??= new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        };
            
        await response.WriteJsonAsync(JsonSerializer.Serialize(o, options), contentType);
    }
        
    public static async Task WriteJsonAsync(this HttpResponse response, string json, string contentType = null)
    {
        response.ContentType = contentType ?? "application/json; charset=UTF-8";
        await response.WriteAsync(json);
        await response.Body.FlushAsync();
    }
}