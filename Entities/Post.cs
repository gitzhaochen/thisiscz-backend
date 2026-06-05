using Microsoft.AspNetCore.Identity;

namespace ThisisczApi.Entities;

public enum PostCategory
{
    Life, // 生活
    Work, // 工作
    Crypto, // 加密货币
    Sports, // 运动
}

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleZh { get; set; }
    public string? Summary { get; set; }
    public string? SummaryZh { get; set; }
    public string? Content { get; set; }
    public string? ContentZh { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public IdentityUser Author { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public PostCategory Category { get; set; }
}
