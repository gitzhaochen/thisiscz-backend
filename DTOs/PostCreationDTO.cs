using System.ComponentModel.DataAnnotations;
using ThisisczApi.Entities;

namespace ThisisczApi.DTOs;

public class PostCreationDTO
{
    [Required(ErrorMessage = "You must fill the {0} field")]
    public PostCategory Category { get; set; }

    [Required(ErrorMessage = "You must fill the {0} field")]
    public string Title { get; set; } = string.Empty;
    public string? TitleZh { get; set; }
    public string? Summary { get; set; }
    public string? SummaryZh { get; set; }
    public string? Content { get; set; }
    public string? ContentZh { get; set; }
}
