namespace STM.Api.Middlewares;

public class LanguageMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var lang = context.Request.Headers.AcceptLanguage.FirstOrDefault() ?? "en";

        context.Items["lang"] = lang;

        await next(context);
    }
}
