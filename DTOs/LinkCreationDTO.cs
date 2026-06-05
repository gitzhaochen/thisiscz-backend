using ThisisczApi.Entities;

namespace ThisisczApi.DTOs;

public class LinkCreationDTO
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public LinkCategory Category { get; set; }
    public string UserId { get; set; } = string.Empty;
}
