namespace Backend.Fx.AspNetCore.ErrorHandling
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;
    using Exceptions;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Http;

    public abstract class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        
        /// <summary>
        ///     This constructor is being called by the framework DI container
        /// </summary>
        [UsedImplicitly]
        protected ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        ///     This method is being called by the previous middleware in the HTTP pipeline
        /// </summary>
        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            if (await ShouldHandle(context))
            {
                try
                {
                    await _next.Invoke(context);
                }
                catch (TooManyRequestsException tmrex)
                {
                    if (tmrex.RetryAfter > 0)
                    {
                        context.Response.Headers.Add("Retry-After", tmrex.RetryAfter.ToString(CultureInfo.InvariantCulture));
                    }

                    await HandleClientError(context, 429, "TooManyRequests", tmrex);
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
                catch (ForbiddenException uex)
                {
                    await HandleClientError(context, (int)HttpStatusCode.Forbidden, HttpStatusCode.Forbidden.ToString(), uex);
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

        protected abstract Task<bool> ShouldHandle(HttpContext context);

        protected abstract Task HandleClientError(HttpContext context, int httpStatusCode, string message, ClientException exception);

        protected abstract Task HandleServerError(HttpContext context, Exception exception);
    }

}
