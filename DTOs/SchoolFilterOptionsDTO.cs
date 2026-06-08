namespace ThisisczApi.DTOs;

public class SchoolFilterOptionsDTO
{
    public List<string> Region { get; set; } = new();
    public List<string> AuthorityClass { get; set; } = new();
    public List<string> OrgType { get; set; } = new();
    public List<string> CoEdStatus { get; set; } = new();
}
