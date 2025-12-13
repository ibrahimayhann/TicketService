using System.Text.Json;

namespace TicketApi.Exceptions;

public sealed class ExceptionMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (RequestValidationException ex)
        {
            await Write(context, ex.StatusCode, ex.Title, ex.Message, ex.Errors);
        }
        catch (ApiException ex)
        {
            await Write(context, ex.StatusCode, ex.Title, ex.Message, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await Write(context, StatusCodes.Status500InternalServerError,
                "Internal server error",
                "Unexpected error occurred.",
                null);
        }
    }

    private static async Task Write(
        HttpContext context,
        int status,
        string title,
        string detail,
        Dictionary<string, string[]>? errors)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        var payload = new ErrorResponse
        {
            Status = status,
            Title = title,
            Detail = detail,
            TraceId = context.TraceIdentifier,
            Errors = errors
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
