using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Hybrid;

namespace AgendeAqui.Api.Middleware;

public sealed class IdempotencyMiddleware(RequestDelegate next)
{
    private const string IdempotencyKeyHeader = "X-Idempotency-Key";
    private static readonly Regex SafeKeyPattern = new(@"^[a-zA-Z0-9\-_.]+$", RegexOptions.Compiled);
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

    public async Task InvokeAsync(HttpContext context, HybridCache cache)
    {
        if (!HttpMethods.IsPost(context.Request.Method) && !HttpMethods.IsPut(context.Request.Method))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var headerValue)
            || string.IsNullOrWhiteSpace(headerValue))
        {
            await next(context);
            return;
        }

        var key = headerValue.ToString();
        if (key.Length > 128 || !SafeKeyPattern.IsMatch(key))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "InvalidIdempotencyKey",
                detail = "X-Idempotency-Key must be 1-128 alphanumeric characters, hyphens, underscores, or dots."
            });
            return;
        }

        var cacheKey = $"idempotency:{key}";

        var cached = await cache.GetOrCreateAsync<IdempotencyEntry?>(
            cacheKey,
            _ => ValueTask.FromResult<IdempotencyEntry?>(null),
            new HybridCacheEntryOptions { Expiration = CacheTtl });

        if (cached is not null)
        {
            context.Response.StatusCode = cached.StatusCode;
            context.Response.ContentType = cached.ContentType ?? "application/json";
            if (cached.Body is { Length: > 0 })
                await context.Response.Body.WriteAsync(cached.Body);
            return;
        }

        var originalBody = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await next(context);

        memoryStream.Seek(0, SeekOrigin.Begin);
        var responseBody = memoryStream.ToArray();

        var entry = new IdempotencyEntry(
            context.Response.StatusCode,
            context.Response.ContentType,
            responseBody);

        await cache.SetAsync(cacheKey, entry, new HybridCacheEntryOptions { Expiration = CacheTtl });

        memoryStream.Seek(0, SeekOrigin.Begin);
        await memoryStream.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }

    private sealed record IdempotencyEntry(int StatusCode, string? ContentType, byte[] Body);
}
