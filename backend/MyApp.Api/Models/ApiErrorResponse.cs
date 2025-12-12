namespace MyApp.Api.Models;

public class ApiErrorResponse : ApiResponseBase
{
    /// <summary>
    /// 驗證錯誤：欄位 -> 多個錯誤訊息
    /// </summary>
    public IDictionary<string, string[]>? Errors { get; set; }
}
