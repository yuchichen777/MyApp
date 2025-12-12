using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyApp.Api.Models;

namespace MyApp.Api.Middleware
{
    public class ApiResponseFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // 只處理 ObjectResult（例如 Ok(), NotFound(), BadRequest()）
            if (context.Result is ObjectResult objectResult)
            {
                var value = objectResult.Value;

                // 已經是 ApiResponse 就跳過
                if (value is ApiResponseBase)
                {
                    await next();
                    return;
                }

                var http = context.HttpContext;
                var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;

                // 處理驗證錯誤 ValidationProblemDetails → 轉 ApiErrorResponse
                if (value is ValidationProblemDetails vpd)
                {
                    objectResult.Value = new ApiErrorResponse
                    {
                        Success = false,
                        StatusCode = statusCode,
                        Message = vpd.Title ?? "驗證失敗",
                        TraceId = http.TraceIdentifier,
                        Errors = vpd.Errors
                    };
                    objectResult.DeclaredType = typeof(ApiErrorResponse);

                    await next();
                    return;
                }

                // 一般成功回傳 Data
                var success = statusCode is >= 200 and < 300;

                var wrapped = new ApiResponse<object?>
                {
                    Success = success,
                    StatusCode = statusCode,
                    Message = null,
                    TraceId = http.TraceIdentifier,
                    Data = value
                };

                objectResult.Value = wrapped;
                objectResult.DeclaredType = typeof(ApiResponse<object?>);
            }
            else if (context.Result is EmptyResult)
            {
                // 沒有內容 (204)
                context.Result = new ObjectResult(new ApiResponse<object?>
                {
                    Success = true,
                    StatusCode = StatusCodes.Status204NoContent,
                    TraceId = context.HttpContext.TraceIdentifier,
                    Data = null
                })
                {
                    StatusCode = StatusCodes.Status204NoContent
                };
            }

            await next();
        }
    }
}
