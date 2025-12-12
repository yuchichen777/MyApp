namespace MyApp.Domain.Entities;

public class UserRefreshToken : AuditableEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public AppUser User { get; set; } = default!;

    /// <summary>
    /// Hash 後的 Refresh Token（不要存明碼）
    /// </summary>
    public string TokenHash { get; set; } = default!;

    /// <summary>
    /// 到期時間（UTC）
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 是否已被明確註銷（例如重新登入 / 登出）
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// 若是 Token Rotation，可記錄是哪一個舊 Token 旋轉過來（選填）
    /// </summary>
    public int? ReplacedByTokenId { get; set; }
}
