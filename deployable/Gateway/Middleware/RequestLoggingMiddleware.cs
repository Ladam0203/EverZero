using ILogger = Serilog.ILogger;

namespace Gateway.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log incoming request information
            Monitoring.Monitoring.Log.Information("Received request: {Method} {Path}", context.Request.Method, context.Request.Path);

            // Create a memory stream to capture the response body
            var originalBodyStream = context.Response.Body;
            using (var memoryStream = new MemoryStream())
            {
                context.Response.Body = memoryStream;

                // Call the next middleware in the pipeline
                await _next(context);

                // Log the result of the request (status code)
                Monitoring.Monitoring.Log.Information("Response status: {StatusCode} for {Method} {Path}",
                    context.Response.StatusCode, context.Request.Method, context.Request.Path);

                // Copy the memory stream back to the original response stream
                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalBodyStream);
            }
        }
    }
}