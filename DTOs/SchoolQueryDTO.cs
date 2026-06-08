namespace ThisisczApi.DTOs;

public class SchoolQueryDTO : PaginationDTO
{
    public string? Name { get; set; }
    public string? Region { get; set; }
    public string? AuthorityClass { get; set; }
    public List<string>? OrgType { get; set; }
    public string? CoEdStatus { get; set; }
    public string EqiIndexSortOrder { get; set; } = "asc";
}
