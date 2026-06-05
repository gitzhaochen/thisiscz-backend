using ThisisczApi.Entities;

namespace ThisisczApi.DTOs;

public class LinkQueryDTO : PaginationDTO
{
    public LinkCategory? Category { get; set; }
}
