namespace ThisisczApi.DTOs;

public class SchoolQueryDTO : PaginationDTO
{
    public string? Name { get; set; }
    public string? City { get; set; }
    public string? AuthorityClass { get; set; }
    public string? LevelClass { get; set; }
    public string? CoEdStatus { get; set; }
    public string EqiIndexSortOrder { get; set; } = "asc";
}
