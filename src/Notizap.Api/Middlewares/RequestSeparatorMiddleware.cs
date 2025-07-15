namespace Notizap.Api.Middlewares
{
    public class RequestSeparatorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestSeparatorMiddleware> _logger;

        public RequestSeparatorMiddleware(RequestDelegate next, ILogger<RequestSeparatorMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // SEPARADOR DE INICIO - Con información del request
            var requestId = context.TraceIdentifier;
            var method = context.Request.Method;
            var path = context.Request.Path;
            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault()?.Split(' ').FirstOrDefault() ?? "Unknown";
            
            _logger.LogInformation(
                "┌─────────────────────────────────────────────────────────────────────────┐");
            _logger.LogInformation(
                "│ 🚀 NUEVA REQUEST: {Method} {Path} | ID: {RequestId} | {UserAgent}",
                method, path, requestId[..8], userAgent); // Solo primeros 8 chars del ID
            _logger.LogInformation(
                "└─────────────────────────────────────────────────────────────────────────┘");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;
                var executionTime = stopwatch.ElapsedMilliseconds;

                // SEPARADOR DE FIN - Con resultado del request
                var statusEmoji = statusCode < 400 ? "✅" : statusCode < 500 ? "⚠️" : "❌";
                
                _logger.LogInformation(
                    "┌─────────────────────────────────────────────────────────────────────────┐");
                _logger.LogInformation(
                    "│ {StatusEmoji} FIN REQUEST: {Method} {Path} | {StatusCode} | {ExecutionTime}ms | ID: {RequestId}",
                    statusEmoji, method, path, statusCode, executionTime, requestId[..8]);
                _logger.LogInformation(
                    "└─────────────────────────────────────────────────────────────────────────┘");
                _logger.LogInformation(""); // Línea vacía para separar
            }
        }
    }
}