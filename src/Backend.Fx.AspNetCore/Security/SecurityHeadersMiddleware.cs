namespace Backend.Fx.AspNetCore.Security
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;


    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IOptions<SecurityHeadersOptions> securityOptionsAccessor;


        [UsedImplicitly]
        public SecurityHeadersMiddleware(RequestDelegate next, IOptions<SecurityHeadersOptions> securityOptionsAccessor)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.securityOptionsAccessor = securityOptionsAccessor;
        }

        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            var csp = securityOptionsAccessor.Value.ContentSecurityPolicy;
            if (csp?.ContentSecurityPolicy != null && csp.ContentSecurityPolicy.Length > 0)
            {
                string cspHeaderKey = csp.ReportOnly ?
                    "Content-Security-Policy-Report-Only" :
                    "Content-Security-Policy";

                string completeCsp = csp.ContentSecurityPolicy;

                if (!string.IsNullOrEmpty(csp.ReportUrl) && ShouldAppendReportUri(context))
                {
                    completeCsp += "; report-uri " + csp.ReportUrl;
                }

                context.Response.Headers.Add(cspHeaderKey, new StringValues(completeCsp));
            }

            if (securityOptionsAccessor.Value.HstsExpiration > 0)
            {
                context.Response.Headers.Add("Strict-Transport-Security", new StringValues($"max-age={securityOptionsAccessor.Value.HstsExpiration}"));
            }

            await next.Invoke(context);
        }

        /// <summary>
        /// Override this if you want the user to be able to permit/forbid reporting
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual bool ShouldAppendReportUri(HttpContext context)
        {
            return true;
        }
    }
}
