namespace MyApp.Api.Models
{
    public class LoggingOptions
    {
        public bool LogRequestBody { get; set; } = false;
        public bool LogResponseBody { get; set; } = false;

        /// <summary>
        /// 最多記錄多少字元，避免 Body 超大
        /// </summary>
        public int MaxBodyLength { get; set; } = 2048;
    }
}
