namespace ThisisczApi.DTOs;

public class SchoolFilterOptionsDTO
{
    public List<string> City { get; set; } = new();
    public List<string> AuthorityClass { get; set; } = new();
    public List<string> LevelClass { get; set; } = new();
    public List<string> CoEdStatus { get; set; } = new();
}
