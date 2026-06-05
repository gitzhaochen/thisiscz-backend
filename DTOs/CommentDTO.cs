namespace ThisisczApi.DTOs;

public class CommentDTO
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public UserDTO User { get; set; } = null!;

    public int PostId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? ParentId { get; set; }
    public CommentDTO? Parent { get; set; }

    public int ReplyCount { get; set; } = 0;
}
