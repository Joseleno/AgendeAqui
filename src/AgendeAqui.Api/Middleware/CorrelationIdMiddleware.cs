using System.Text.RegularExpressions;
using Serilog.Context;

namespace AgendeAqui.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private static readonly Regex SafeCorrelationIdPattern = new(@"^[a-zA-Z0-9\-_.]+$", RegexOptions.Compiled);

    public async Task InvokeAsync(HttpContext context)
    {
        var headerValue = context.Request.Headers.ContainsKey(CorrelationIdHeader)
            ? context.Request.Headers[CorrelationIdHeader].ToString()
            : null;

        var correlationId = !string.IsNullOrWhiteSpace(headerValue)
            && headerValue.Length <= 64
            && SafeCorrelationIdPattern.IsMatch(headerValue)
            ? headerValue
            : Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
