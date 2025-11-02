namespace KQAlumni.API.Middleware;

/// <summary>
/// Middleware to track requests with unique IDs for debugging and audit trails
/// Generates or accepts X-Request-ID header and adds it to responses
/// </summary>
public class RequestIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestIdMiddleware> _logger;
    private const string RequestIdHeaderName = "X-Request-ID";
    private const string RequestIdKey = "RequestId";

    public RequestIdMiddleware(
        RequestDelegate next,
        ILogger<RequestIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or generate request ID
        string requestId = context.Request.Headers[RequestIdHeaderName].FirstOrDefault()
                           ?? Guid.NewGuid().ToString();

        // Store in HttpContext.Items for access throughout the request pipeline
        context.Items[RequestIdKey] = requestId;

        // Add to response headers
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(RequestIdHeaderName))
            {
                context.Response.Headers.TryAdd(RequestIdHeaderName, requestId);
            }
            return Task.CompletedTask;
        });

        // Log request details with request ID
        _logger.LogInformation(
            "Request {Method} {Path} started with RequestId: {RequestId}",
            context.Request.Method,
            context.Request.Path,
            requestId);

        try
        {
            await _next(context);
        }
        finally
        {
            // Log response details with request ID
            _logger.LogInformation(
                "Request {Method} {Path} completed with status {StatusCode} - RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                requestId);
        }
    }
}
