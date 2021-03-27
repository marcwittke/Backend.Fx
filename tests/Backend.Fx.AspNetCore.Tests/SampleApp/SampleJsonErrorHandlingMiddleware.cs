using Backend.Fx.AspNetCore.ErrorHandling;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore.Tests.SampleApp
{
    public class SampleJsonErrorHandlingMiddleware : JsonErrorHandlingMiddleware
    {
        public SampleJsonErrorHandlingMiddleware(RequestDelegate next) : base(next, showInternalServerErrorDetails: true)
        {
        }
    }
}