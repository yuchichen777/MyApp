namespace MyApp.Api.Models
{
    public abstract class ApiResponseBase
    {
        /// <summary>
        /// 是否成功，一律 true
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// HTTP 狀態碼，例如 200, 201, 204
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 提示訊息（可選）
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 追蹤用 Id，可對應 log
        /// </summary>
        public string? TraceId { get; set; }
    }
}
