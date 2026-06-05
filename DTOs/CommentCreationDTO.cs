using System.ComponentModel.DataAnnotations;

namespace ThisisczApi.DTOs;

public class CommentCreationDTO
{
    [Required(ErrorMessage = "You must fill the {0} field")]
    public int PostId { get; set; }

    public int? ParentId { get; set; }

    [Required(ErrorMessage = "You must fill the {0} field")]
    [StringLength(maximumLength: 500)]
    public required string Content { get; set; } = string.Empty;
}
