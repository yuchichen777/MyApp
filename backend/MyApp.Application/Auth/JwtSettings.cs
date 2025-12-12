namespace MyApp.Application.Auth;

public class JwtSettings
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;

    /// <summary>
    /// 用於簽署 Access Token 的金鑰（原本的 Key）
    /// </summary>
    public string Key { get; set; } = default!;

    /// <summary>
    /// Access Token 有效時間（分鐘）
    /// </summary>
    public int AccessTokenExpiryMinutes { get; set; } = 30; // 預設 30 分鐘

    /// <summary>
    /// Refresh Token 有效時間（天）
    /// </summary>
    public int RefreshTokenExpiryDays { get; set; } = 7;    // 預設 7 天
}
