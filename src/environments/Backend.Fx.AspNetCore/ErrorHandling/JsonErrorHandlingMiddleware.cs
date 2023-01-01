using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.Fx.Exceptions;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace Backend.Fx.AspNetCore.ErrorHandling;

[PublicAPI]
public class JsonErrorHandlingMiddleware : ErrorHandlingMiddleware
{
    private readonly bool _showInternalServerErrorDetails;
    private readonly ILogger _logger = Log.Create<JsonErrorHandlingMiddleware>();

    private static readonly JsonSerializerOptions SerializerOptions =
        new JsonSerializerOptions { WriteIndented = true }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);


    public JsonErrorHandlingMiddleware(RequestDelegate next, bool showInternalServerErrorDetails)
        : base(next)
    {
        _showInternalServerErrorDetails = showInternalServerErrorDetails;
    }

    protected override Task<bool> ShouldHandle(HttpContext context)
    {
        // this middleware only handles requests that accept json as response
        IList<MediaTypeHeaderValue> accept = context.Request.GetTypedHeaders().Accept;
        return Task.FromResult(accept.Any(mth => mth.Type == "application" && mth.SubType == "json"));
    }

    protected override async Task HandleClientError(HttpContext context, int httpStatusCode, string message,
        ClientException exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("exception cannot be handled correctly, because the response has already started");
            return;
        }

        // convention: only the errors array will be transmitted to the client, allowing technical (possibly
        // revealing) information in the exception message.
        Errors errors = exception.HasErrors()
            ? exception.Errors
            : new Errors($"HTTP{httpStatusCode}: {message}");

        context.Response.StatusCode = httpStatusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(new ErrorResponse(errors).ToJsonString());
    }

    protected override async Task HandleServerError(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("exception cannot be handled correctly, because the response has already started");
            return;
        }

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var responseContent = _showInternalServerErrorDetails
            ? JsonSerializer.Serialize(new { message = exception.Message, stackTrace = exception.StackTrace }, SerializerOptions)
            : JsonSerializer.Serialize(new { message = "An internal error occured" }, SerializerOptions);
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(responseContent);
    }
}