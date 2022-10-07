using System;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Backend.Fx.AspNetCore.Versioning;

[PublicAPI]
public class VersionHeaderMiddleware 
{
    private readonly RequestDelegate _next;
    private readonly string _assemblyName;
    private readonly string _version;

    public VersionHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null)
        {
            throw new InvalidOperationException("Unable to determine the entry assembly. The Version Header Middleware cannot be used in this environment");
        }

        AssemblyName entryAssemblyName = entryAssembly.GetName();
        if (entryAssemblyName.Version == null)
        {
            throw new InvalidOperationException("Unable to determine the version of the entry assembly. The Version Header Middleware cannot be used in this environment");
        }

        _assemblyName = entryAssemblyName.Name;
        _version = entryAssemblyName.Version.ToString(3);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Add(_assemblyName, new StringValues(_version));
        await _next.Invoke(context);
    }
}