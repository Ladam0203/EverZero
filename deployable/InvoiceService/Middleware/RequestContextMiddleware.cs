namespace InvoiceService.Middleware;

/// <summary>
/// A middleware that wraps the current <see cref="HttpContext"/> in a <see cref="RequestContext"/> object.
/// </summary>
public class RequestContextMiddleware
{
    private readonly RequestDelegate _next;

    public RequestContextMiddleware(RequestDelegate next) {
        _next = next;
    }

    /// <summary>
    /// Invokes the current context middleware.
    /// </summary>
    /// <param name="httpContext">The HTTP context received from the Http Request.</param>
    /// <param name="requestContext">The current context to build.</param>
    public async Task Invoke(HttpContext httpContext, RequestContext requestContext) {
        requestContext.Build(httpContext);
        await _next.Invoke(httpContext);
    }
}