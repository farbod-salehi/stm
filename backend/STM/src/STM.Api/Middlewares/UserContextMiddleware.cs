namespace STM.Api.Middlewares;

public sealed class UserContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst("userId");

            if (userIdClaim is not null)
            {
                context.Items["userId"] = userIdClaim.Value;
            }
        }

        await next(context);
    }
}
