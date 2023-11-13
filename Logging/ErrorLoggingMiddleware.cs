namespace MeteredPDUWebNew.Logging;

public class ErrorLoggingMiddleware
{
    private RequestDelegate _next;
    private ILogger<ErrorLoggingMiddleware> _logger;

    public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            _logger.LogError($"Error in request: {e}");
            throw;
        }
    }
}