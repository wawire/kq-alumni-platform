using KQAlumni.Core.DTOs;
using System.Net;
using System.Text.Json;

namespace KQAlumni.API.Middleware;

/// <summary>
/// Global error handling middleware
/// Catches unhandled exceptions and returns standardized error responses
/// </summary>
public class ErrorHandlingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<ErrorHandlingMiddleware> _logger;
  private readonly IHostEnvironment _environment;

  public ErrorHandlingMiddleware(
      RequestDelegate next,
      ILogger<ErrorHandlingMiddleware> logger,
      IHostEnvironment environment)
  {
    _next = next;
    _logger = logger;
    _environment = environment;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
      await HandleExceptionAsync(context, ex);
    }
  }

  private async Task HandleExceptionAsync(HttpContext context, Exception exception)
  {
    context.Response.ContentType = "application/json";

    var errorResponse = new ErrorResponse
    {
      Timestamp = DateTime.UtcNow
    };

    switch (exception)
    {
      case UnauthorizedAccessException:
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        errorResponse.Type = "https://tools.ietf.org/html/rfc7235#section-3.1";
        errorResponse.Title = "Unauthorized";
        errorResponse.Status = (int)HttpStatusCode.Unauthorized;
        errorResponse.Detail = "You are not authorized to perform this action";
        break;

      case KeyNotFoundException:
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
        errorResponse.Title = "Resource not found";
        errorResponse.Status = (int)HttpStatusCode.NotFound;
        errorResponse.Detail = exception.Message;
        break;

      case InvalidOperationException:
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
        errorResponse.Title = "Invalid operation";
        errorResponse.Status = (int)HttpStatusCode.BadRequest;
        errorResponse.Detail = exception.Message;
        break;

      default:
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        errorResponse.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
        errorResponse.Title = "Internal server error";
        errorResponse.Status = (int)HttpStatusCode.InternalServerError;
        errorResponse.Detail = _environment.IsDevelopment()
            ? exception.Message
            : "An unexpected error occurred. Please try again later or contact support.";
        break;
    }

    var jsonOptions = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
    await context.Response.WriteAsync(json);
  }
}
