using ThisisczApi.Entities;

namespace ThisisczApi.DTOs;

public class PostQueryDTO : PaginationDTO
{
    public PostCategory? Category { get; set; }
}
