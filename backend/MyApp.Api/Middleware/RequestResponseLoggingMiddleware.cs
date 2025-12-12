using Microsoft.Extensions.Options;
using MyApp.Api.Models;
using System.Text;

namespace MyApp.Api.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly LoggingOptions _options;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger,
        IOptions<LoggingOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 先把 OPTIONS 直接放行，不記任何 Body
        if (context.Request.Method == HttpMethods.Options)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        var isJsonRequest = context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true
                            || context.Request.ContentType?.StartsWith("text/", StringComparison.OrdinalIgnoreCase) == true;

        // 記 RequestBody，但排除 /api/Auth/login（避免密碼）
        var isSensitiveLoginPath = path.StartsWith("/api/Auth/login", StringComparison.OrdinalIgnoreCase);

        if (_options.LogRequestBody && isJsonRequest && !isSensitiveLoginPath)
        {
            context.Request.EnableBuffering();

            string requestBody;
            using (var reader = new StreamReader(
                       context.Request.Body,
                       encoding: Encoding.UTF8,
                       detectEncodingFromByteOrderMarks: false,
                       bufferSize: 1024,
                       leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            if (requestBody.Length > _options.MaxBodyLength)
            {
                requestBody = requestBody[.._options.MaxBodyLength] + "...(truncated)";
            }

            _logger.LogInformation(
                "[RequestBody] {Method} {Path} Body={Body}",
                context.Request.Method,
                path,
                requestBody);
        }

        // 換掉 Response Body 並攔截
        var originalBodyStream = context.Response.Body;
        await using var newBodyStream = new MemoryStream();
        context.Response.Body = newBodyStream;

        try
        {
            await _next(context);
        }
        finally
        {
            if (_options.LogResponseBody)
            {
                newBodyStream.Position = 0;
                string responseBody = await new StreamReader(newBodyStream).ReadToEndAsync();

                if (responseBody.Length > _options.MaxBodyLength)
                {
                    responseBody = responseBody[.._options.MaxBodyLength] + "...(truncated)";
                }

                _logger.LogInformation(
                    "[ResponseBody] {StatusCode} {Method} {Path} Body={Body}",
                    context.Response.StatusCode,
                    context.Request.Method,
                    path,
                    responseBody);
            }

            newBodyStream.Position = 0;
            await newBodyStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
    }
}
