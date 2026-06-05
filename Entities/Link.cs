using Microsoft.AspNetCore.Identity;

namespace ThisisczApi.Entities;

public enum LinkCategory
{
    Life, // 生活
    Work, // 工作
    Crypto, // 加密货币
    Sports, // 运动
    Movies, // 电影
}

public class Link
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public LinkCategory Category { get; set; }
    public string UserId { get; set; } = string.Empty;
    public IdentityUser User { get; set; } = null!;
}
