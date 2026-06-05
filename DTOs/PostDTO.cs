using ThisisczApi.Entities;

namespace ThisisczApi.DTOs;

public class PostDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleZh { get; set; }
    public string? Summary { get; set; }
    public string? SummaryZh { get; set; }
    public string? Content { get; set; }
    public string? ContentZh { get; set; }
    public UserDTO Author { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public int LikeCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public int CommentCount { get; set; }
    public PostCategory Category { get; set; }
}
