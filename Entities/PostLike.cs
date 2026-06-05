using Microsoft.AspNetCore.Identity;

namespace ThisisczApi.Entities;

public class PostLike
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
