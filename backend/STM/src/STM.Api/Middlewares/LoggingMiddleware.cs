using System.Diagnostics;

namespace STM.Api.Middlewares;

public sealed class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        logger.LogInformation("Request started {Method} {Path}", request.Method, request.Path);

        await next(context);

        stopwatch.Stop();
        var response = context.Response;

        logger.LogInformation(
            "Request finished {Method} {Path} with {StatusCode} in {ElapsedMilliseconds} ms",
            request.Method,
            request.Path,
            response.StatusCode,
            stopwatch.ElapsedMilliseconds
        );
    }
}