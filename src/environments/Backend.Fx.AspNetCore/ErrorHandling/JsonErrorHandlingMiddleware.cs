namespace Backend.Fx.AspNetCore.ErrorHandling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Exceptions;
    using JetBrains.Annotations;
    using Logging;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class JsonErrorHandlingMiddleware
    {
        private static readonly ILogger Logger = LogManager.Create<JsonErrorHandlingMiddleware>();
        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _env;

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = true }
            },
        };

        /// <summary>
        ///     This constructor is being called by the framework DI container
        /// </summary>
        [UsedImplicitly]
        public JsonErrorHandlingMiddleware(RequestDelegate next, IHostingEnvironment env)
        {
            this._next = next;
            this._env = env;
        }

        /// <summary>
        ///     This method is being called by the previous middleware in the HTTP pipeline
        /// </summary>
        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            // this middleware only handles requests that accept json as response
            IList<MediaTypeHeaderValue> accept = context.Request.GetTypedHeaders().Accept;
            if (accept?.Any(mth => mth.Type == "application" && mth.SubType == "json") == true) 
            {
                try
                {
                    await _next.Invoke(context);
                }
                catch (UnprocessableException uex)
                {
                    await HandleClientError(context, 422, "Unprocessable", uex);
                }
                catch (NotFoundException nfex)
                {
                    await HandleClientError(context, (int)HttpStatusCode.NotFound, HttpStatusCode.NotFound.ToString(), nfex);
                }
                catch (ConflictedException confex)
                {
                    await HandleClientError(context, (int)HttpStatusCode.Conflict, HttpStatusCode.Conflict.ToString(), confex);
                }
                catch (UnauthorizedException uex)
                {
                    await HandleClientError(context, (int)HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized.ToString(), uex);
                }
                catch (ClientException cex)
                {
                    await HandleClientError(context, (int)HttpStatusCode.BadRequest, HttpStatusCode.BadRequest.ToString(), cex);
                }
                catch (Exception ex)
                {
                    await HandleServerError(context, ex);
                }
            }
            else
            {
                await _next.Invoke(context);
            }
        }
        
        private async Task HandleClientError(HttpContext context, int httpStatusCode, string code, ClientException exception)
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
                                    : new Errors { new Error($"HTTP{httpStatusCode}", code) };

            context.Response.StatusCode = httpStatusCode;
            string responseContent = JsonConvert.SerializeObject(new { errors }, _jsonSerializerSettings);
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(responseContent);
        }

        private async Task HandleServerError(HttpContext context, Exception exception)
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
    }
}
