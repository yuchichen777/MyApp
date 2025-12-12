namespace MyApp.Api.Models;

public class ApiResponse<T> : ApiResponseBase
{
    /// <summary>
    /// 實際資料
    /// </summary>
    public T? Data { get; set; }
}
