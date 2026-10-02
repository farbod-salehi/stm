namespace STM.Api.Middlewares;

public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var payload = new
            {
                success = false,
                messageKey = "INTERNAL_SERVER_ERROR",
                statusCode = StatusCodes.Status500InternalServerError
            };

            await context.Response.WriteAsJsonAsync(payload);
        }
    }
}
