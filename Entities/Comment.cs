using Microsoft.AspNetCore.Identity;

namespace ThisisczApi.Entities;

public class Comment
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public IdentityUser User { get; set; } = null!;
    public int PostId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Self reference
    public int? ParentId { get; set; }
    public Comment? Parent { get; set; }
}
