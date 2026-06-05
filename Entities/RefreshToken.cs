namespace ThisisczApi.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 指示 refresh token 是否被撤销（用于实现如用户主动登出、管理后台强制令牌失效、异常检测等场景，防止已失效或被撤销的 token 换取新的 access token）
    public bool IsRevoked { get; set; } = false;
}
