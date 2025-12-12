using FluentValidation;
using MyApp.Api.Models;
using System.Net;
using System.Text.Json;

namespace MyApp.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException ex)
        {
            // 驗證錯誤：用 Warning + 明確標註是哪個 API
            _logger.LogWarning(ex,
                "[ValidationException] {Message} | {Method} {Path} | TraceId={TraceId}",
                ex.Message,
                context.Request?.Method,
                context.Request?.Path.Value,
                context.TraceIdentifier);

            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var response = new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "驗證失敗",
                TraceId = context.TraceIdentifier,
                Errors = errors
            };

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json; charset=utf-8";

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
        catch (Exception ex)
        {
            // 未預期錯誤：用 Error + Method/Path + TraceId
            _logger.LogError(ex,
                "[Exception] {Message} | {Method} {Path} | TraceId={TraceId}",
                ex.Message,
                context.Request?.Method,
                context.Request?.Path.Value,
                context.TraceIdentifier);

            var response = new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "系統發生未預期錯誤，請聯絡系統管理員。",
                TraceId = context.TraceIdentifier
            };

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}
