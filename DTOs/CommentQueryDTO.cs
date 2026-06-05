using System.ComponentModel.DataAnnotations;

namespace ThisisczApi.DTOs;

public class CommentQueryDTO : PaginationDTO
{
    [Required(ErrorMessage = "You must fill the {0} field")]
    public int PostId { get; set; }
    public int? ParentId { get; set; }
}
