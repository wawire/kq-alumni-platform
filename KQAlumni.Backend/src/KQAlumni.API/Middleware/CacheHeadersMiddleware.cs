namespace KQAlumni.API.Middleware;

/// <summary>
/// Middleware to add appropriate caching headers to responses
/// Implements cache policies based on endpoint patterns
/// </summary>
public class CacheHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CacheHeadersMiddleware> _logger;

    public CacheHeadersMiddleware(
        RequestDelegate next,
        ILogger<CacheHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
            var method = context.Request.Method;

            // Only apply caching to GET requests
            if (method != "GET")
            {
                AddNoCacheHeaders(context);
                return Task.CompletedTask;
            }

            // Apply different cache policies based on endpoint
            if (path.Contains("/health"))
            {
                // Health checks: no cache
                AddNoCacheHeaders(context);
            }
            else if (path.Contains("/api/v1/registrations/status") ||
                     path.Contains("/api/v1/registrations/check"))
            {
                // Status checks: short cache (1 minute)
                AddCacheHeaders(context, 60, isPublic: true);
            }
            else if (path.Contains("/api/v1/admin/registrations"))
            {
                // Admin endpoints: no cache (sensitive data)
                AddNoCacheHeaders(context);
            }
            else if (path.Contains("/swagger") || path.Contains("/api-docs"))
            {
                // API documentation: medium cache (5 minutes)
                AddCacheHeaders(context, 300, isPublic: true);
            }
            else if (path.Contains("/api/"))
            {
                // Other API endpoints: short cache with revalidation (2 minutes)
                AddCacheHeadersWithRevalidation(context, 120);
            }
            else
            {
                // Default: no explicit caching
                AddNoCacheHeaders(context);
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }

    private void AddNoCacheHeaders(HttpContext context)
    {
        context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";
    }

    private void AddCacheHeaders(HttpContext context, int maxAgeSeconds, bool isPublic = false)
    {
        var visibility = isPublic ? "public" : "private";
        context.Response.Headers["Cache-Control"] = $"{visibility}, max-age={maxAgeSeconds}";
        context.Response.Headers["Expires"] = DateTime.UtcNow.AddSeconds(maxAgeSeconds).ToString("R");

        // Add ETag if response has content
        if (context.Response.ContentLength.HasValue && context.Response.ContentLength > 0)
        {
            var etag = $"\"{Guid.NewGuid():N}\"";
            context.Response.Headers["ETag"] = etag;
        }
    }

    private void AddCacheHeadersWithRevalidation(HttpContext context, int maxAgeSeconds)
    {
        context.Response.Headers["Cache-Control"] = $"private, max-age={maxAgeSeconds}, must-revalidate";
        context.Response.Headers["Expires"] = DateTime.UtcNow.AddSeconds(maxAgeSeconds).ToString("R");

        // Add ETag for conditional requests
        if (context.Response.ContentLength.HasValue && context.Response.ContentLength > 0)
        {
            var etag = $"\"{Guid.NewGuid():N}\"";
            context.Response.Headers["ETag"] = etag;
        }
    }
}
