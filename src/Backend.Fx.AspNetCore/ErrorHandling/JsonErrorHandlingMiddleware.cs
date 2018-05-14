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
        private readonly RequestDelegate next;
        private readonly IHostingEnvironment env;

        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
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
            this.next = next;
            this.env = env;
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
                    await next.Invoke(context);
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
                await next.Invoke(context);
            }
        }
        
        private async Task HandleClientError(HttpContext context, int httpStatusCode, string code, ClientException exception)
        {
            Logger.Warn(exception);
            
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
            string responseContent = JsonConvert.SerializeObject(new { errors }, jsonSerializerSettings);
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(responseContent);
        }

        private async Task HandleServerError(HttpContext context, Exception exception)
        {
            Logger.Error(exception);
            if (context.Response.HasStarted)
            {
                Logger.Warn("exception cannot be handled correctly, because the response has already started");
                return;
            }
            context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            var responseContent = env.IsDevelopment()
                                          ? JsonConvert.SerializeObject(new { message = exception.Message, stackTrace = exception.StackTrace }, jsonSerializerSettings)
                                          : JsonConvert.SerializeObject(new { message = "An internal error occured" }, jsonSerializerSettings);
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(responseContent);
        }
    }
}
