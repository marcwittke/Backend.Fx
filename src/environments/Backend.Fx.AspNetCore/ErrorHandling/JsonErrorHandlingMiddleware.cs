namespace Backend.Fx.AspNetCore.ErrorHandling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Exceptions;
    using Logging;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class JsonErrorHandlingMiddleware : ErrorHandlingMiddleware
    {
        private readonly IHostingEnvironment _env;
        private static readonly ILogger Logger = LogManager.Create<JsonErrorHandlingMiddleware>();
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = true }
            },
        };

        public JsonErrorHandlingMiddleware(RequestDelegate next, IHostingEnvironment env) 
            : base(next)
        {
            _env = env;
        }

        protected override Task<bool> ShouldHandle(HttpContext context)
        {
            // this middleware only handles requests that accept json as response
            IList<MediaTypeHeaderValue> accept = context.Request.GetTypedHeaders().Accept;
            return Task.FromResult(accept?.Any(mth => mth.Type == "application" && mth.SubType == "json") == true);
        }

        protected override async Task HandleClientError(HttpContext context, int httpStatusCode, string message, ClientException exception)
        {
            if (context.Response.HasStarted)
            {
                Logger.Warn("exception cannot be handled correctly, because the response has already started");
                return;
            }
            
            // convention: only the errors array will be transmitted to the client, allowing technical (possibly
            // revealing) information in the exception message.
            Errors errors = exception.HasErrors()
                ? exception.Errors
                : new Errors().Add($"HTTP{httpStatusCode}: {message}");

            context.Response.StatusCode = httpStatusCode;
            string serializedErrors = SerializeErrors(errors);
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(serializedErrors);
        }

        protected override async Task HandleServerError(HttpContext context, Exception exception)
        {
            if (context.Response.HasStarted)
            {
                Logger.Warn("exception cannot be handled correctly, because the response has already started");
                return;
            }
            context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            var responseContent = _env.IsDevelopment()
                                          ? JsonConvert.SerializeObject(new { message = exception.Message, stackTrace = exception.StackTrace }, _jsonSerializerSettings)
                                          : JsonConvert.SerializeObject(new { message = "An internal error occured" }, _jsonSerializerSettings);
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(responseContent);
        }

        protected virtual string SerializeErrors(Errors errors)
        {
            var errorsDictionaryForJson = errors.ToDictionary(kvp => kvp.Key == "" ? "_error" : kvp.Key, kvp => kvp.Value);
            return JsonConvert.SerializeObject(errorsDictionaryForJson, _jsonSerializerSettings);
        }
    }
}
